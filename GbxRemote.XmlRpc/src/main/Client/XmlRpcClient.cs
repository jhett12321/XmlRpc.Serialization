using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GbxRemote.XmlRpc.Serialization;
using GbxRemote.XmlRpc.Serialization.Models;

namespace GbxRemote.XmlRpc.Client;

public sealed class XmlRpcClient(string host, int port) : IDisposable
{
  private const uint RequestIdMin = 0x80000000 - 1;

  private static readonly GbxHeader ConnectHeader = new GbxHeader("GBXRemote 2");
  private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);

  public string Host { get; } = host;
  public int Port { get; } = port;

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

  public async Task<T> PostAsync<T>(XmlRpcRequestMessage requestMessage)
  {
    if (!tcpClient.Connected)
    {
      throw new InvalidOperationException("The client is not connected.");
    }

    byte[] requestData = XmlRpcSerializer.SerializeRequest(requestMessage);
    XmlRpcRequest request = new XmlRpcRequest
    {
      RequestId = GetRequestId(),
      Tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously),
      ReceiveResponse = (stream, info) => ReceiveResponse<T>(stream, info)!,
    };

    if (!activeRequests.TryAdd(request.RequestId, request))
    {
      throw new Exception($"Failed to add request '{request.RequestId}'");
    }

    NetworkStream netStream = tcpClient.GetStream();
    SendRequestInfo(netStream, request, requestData);
    await SendRequest(netStream, requestData);

    return (T)await request.Tcs.Task;
  }

  // TODO: Add id overflow test
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

  private void SendRequestInfo(Stream stream, XmlRpcRequest request, byte[] requestData)
  {
    Span<byte> buffer = stackalloc byte[sizeof(int) + sizeof(uint)];
    BinaryPrimitives.WriteInt32LittleEndian(buffer, requestData.Length);
    BinaryPrimitives.WriteUInt32LittleEndian(buffer[sizeof(int)..], request.RequestId);

    stream.Write(buffer);
  }

  private async Task SendRequest(Stream stream, byte[] requestData)
  {
    await stream.WriteAsync(requestData);
  }

  private void ReceiveLoop()
  {
    NetworkStream netStream = tcpClient.GetStream();
    while (tcpClient.Connected && receiveLoopCancel != null && !receiveLoopCancel.IsCancellationRequested)
    {
      XmlRpcResponseInfo info = ReceiveResponseInfo(netStream);
      if (activeRequests.Remove(info.ResponseId, out XmlRpcRequest? pendingRequest))
      {
        try
        {
          object response = pendingRequest.ReceiveResponse(netStream, info);
          pendingRequest.Tcs.SetResult(response);
        }
        catch (Exception e)
        {
          pendingRequest.Tcs.SetException(e);
        }
      }

      // TODO: Callbacks
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

  private T ReceiveResponse<T>(Stream stream, XmlRpcResponseInfo info)
  {
    byte[] buffer = new byte[info.Size];
    stream.ReadExactly(buffer);

    return XmlRpcSerializer.DeserializeResponse<T>(buffer);
  }

  public void Dispose()
  {
    receiveLoopCancel?.Cancel();

    tcpClient.Dispose();
    receiveLoopCancel?.Dispose();

    receiveLoop = null;
    receiveLoopCancel = null;
  }

  private sealed class XmlRpcRequest
  {
    public required uint RequestId { get; init; }
    public required TaskCompletionSource<object> Tcs { get; init; }
    public required Func<Stream, XmlRpcResponseInfo, object> ReceiveResponse { get; init; }
  }

  private struct XmlRpcResponseInfo
  {
    public int Size { get; set; }
    public uint ResponseId { get; set; }
  }
}
