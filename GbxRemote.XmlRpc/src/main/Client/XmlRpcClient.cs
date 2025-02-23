using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GbxRemote.XmlRpc.Serialization;
using GbxRemote.XmlRpc.Serialization.Converters;
using GbxRemote.XmlRpc.Serialization.Models;

namespace GbxRemote.XmlRpc.Client;

public sealed class XmlRpcClient(string host, int port) : IDisposable
{
  private const uint RequestIdMin = 0x80000000 - 1;

  private static readonly GbxHeader ConnectHeader = new GbxHeader("GBXRemote 2");
  private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);

  public string Host { get; } = host;
  public int Port { get; } = port;
  public event Action<XmlRpcResponseInfo, byte[]>? OnUnhandledResponse;

  private readonly TcpClient tcpClient = new TcpClient();
  private readonly ConcurrentDictionary<uint, XmlRpcRequest> activeRequests = new ConcurrentDictionary<uint, XmlRpcRequest>();

  private CancellationTokenSource? receiveLoopCancel;
  private Thread? receiveLoop;

  private uint currentRequestId = RequestIdMin;

  public async Task ConnectAsync()
  {
    CancellationTokenSource timeoutCts = new CancellationTokenSource(ConnectTimeout);

    await tcpClient.ConnectAsync(Host, Port, timeoutCts.Token);

    if (!tcpClient.Connected)
    {
      throw new Exception($"Failed to connect to {Host}:{Port}");
    }

    NetworkStream stream = tcpClient.GetStream();
    await ConnectHeader.VerifyFromStreamAsync(stream, timeoutCts.Token);

    receiveLoopCancel = new CancellationTokenSource();
    receiveLoop = new Thread(ReceiveLoop)
    {
      IsBackground = true,
      Name = $"{nameof(XmlRpcClient)} {nameof(ReceiveLoop)}",
    };

    receiveLoop.Start();
  }

  public async Task<T> PostRequestAsync<T>(XmlRpcRequestMessage requestMessage, XmlRpcValueConverter<T>? responseConverter = null)
  {
    if (!tcpClient.Connected)
    {
      throw new InvalidOperationException("The client is not connected.");
    }

    responseConverter ??= XmlRpcConverterFactory.GetBuiltInValueConverter<T>();
    byte[] requestData = XmlRpcSerializer.SerializeRequest(requestMessage);

    XmlRpcRequest<T> request = new XmlRpcRequest<T>
    {
      RequestId = GetRequestId(),
      Task = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously),
    };
    request.ReceiveResponse = responseData => ReceiveResponse(request, responseData, responseConverter);

    if (!activeRequests.TryAdd(request.RequestId, request))
    {
      throw new Exception($"Failed to add request '{request.RequestId}'");
    }

    await SendAsync(request.RequestId, requestData);

    return await request.Task.Task;
  }

  public async Task PostResponseAsync<T>(T response, uint responseId, XmlRpcValueConverter<T>? responseConverter = null)
  {
    responseConverter ??= XmlRpcConverterFactory.GetBuiltInValueConverter<T>();
    byte[] responseData = XmlRpcSerializer.SerializeResponse(response, responseConverter);

    await SendAsync(responseId, responseData);
  }

  private uint GetRequestId()
  {
    uint id = Interlocked.Increment(ref currentRequestId);

    // At uint.MaxValue, Interlocked.Increment will overflow to uint.MinValue.
    if (id > RequestIdMin)
    {
      return id;
    }

    Interlocked.Exchange(ref currentRequestId, RequestIdMin);
    return Interlocked.Increment(ref currentRequestId);
  }

  private async Task SendAsync(uint requestId, byte[] requestData)
  {
    NetworkStream netStream = tcpClient.GetStream();
    SendRequestInfo(netStream, requestId, requestData);

    await netStream.WriteAsync(requestData);
  }

  private void SendRequestInfo(Stream stream, uint requestId, byte[] requestData)
  {
    Span<byte> buffer = stackalloc byte[sizeof(int) + sizeof(uint)];
    BinaryPrimitives.WriteInt32LittleEndian(buffer, requestData.Length);
    BinaryPrimitives.WriteUInt32LittleEndian(buffer[sizeof(int)..], requestId);

    stream.Write(buffer);
  }

  private void ReceiveLoop()
  {
    NetworkStream netStream = tcpClient.GetStream();
    while (tcpClient.Connected && receiveLoopCancel != null && !receiveLoopCancel.IsCancellationRequested)
    {
      XmlRpcResponseInfo info = ReceiveResponseInfo(netStream);
      byte[] responseData = new byte[info.Size];
      netStream.ReadExactly(responseData);

      if (activeRequests.Remove(info.ResponseId, out XmlRpcRequest? pendingRequest))
      {
        pendingRequest.ReceiveResponse(responseData);
      }
      else
      {
        OnUnhandledResponse?.Invoke(info, responseData);
      }
    }

    receiveLoop = null;
    receiveLoopCancel = null;
  }

  private XmlRpcResponseInfo ReceiveResponseInfo(Stream stream)
  {
    XmlRpcResponseInfo info = new XmlRpcResponseInfo();
    Span<byte> buffer = stackalloc byte[sizeof(int)];

    // Response Size
    stream.ReadExactly(buffer);
    info.Size = BinaryPrimitives.ReadInt32LittleEndian(buffer);

    // Response Id
    stream.ReadExactly(buffer);
    info.ResponseId = BinaryPrimitives.ReadUInt32LittleEndian(buffer);

    return info;
  }

  private void ReceiveResponse<T>(XmlRpcRequest<T> request, byte[] serializedXml, XmlRpcValueConverter<T> responseConverter)
  {
    try
    {
      T value = XmlRpcSerializer.DeserializeResponse(serializedXml, responseConverter);
      request.Task.SetResult(value);
    }
    catch (Exception e)
    {
      request.Task.SetException(e);
    }
  }

  public void Dispose()
  {
    receiveLoopCancel?.Cancel();

    tcpClient.Dispose();
    receiveLoopCancel?.Dispose();

    receiveLoop = null;
    receiveLoopCancel = null;
  }

  private class XmlRpcRequest
  {
    public required uint RequestId { get; init; }
    public Action<byte[]> ReceiveResponse { get; set; } = null!;
  }

  private sealed class XmlRpcRequest<T> : XmlRpcRequest
  {
    public required TaskCompletionSource<T> Task { get; init; }
  }
}
