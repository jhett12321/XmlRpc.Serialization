using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GbxRemote.XmlRpc.Client;

internal sealed class GbxHeader(string header)
{
  private readonly byte[] headerBytes = Encoding.UTF8.GetBytes(header);

  public async Task VerifyFromStreamAsync(Stream stream, CancellationToken token = default)
  {
    byte[] buffer = new byte[4];
    await stream.ReadExactlyAsync(buffer, token);

    int headerLength = BinaryPrimitives.ReadInt32LittleEndian(buffer);
    if (headerLength != headerBytes.Length)
    {
      throw new InvalidOperationException($"Invalid header length, expected '{headerBytes.Length}', got '{headerLength}'");
    }

    buffer = new byte[headerLength];
    await stream.ReadExactlyAsync(buffer, token);

    if (!IsMatchingHeader(buffer, headerBytes))
    {
      throw new InvalidOperationException($"Invalid header, expected '{header}', got '{Encoding.UTF8.GetString(buffer)}'");
    }
  }

  private static bool IsMatchingHeader(ReadOnlySpan<byte> actualHeader, ReadOnlySpan<byte> expectedHeader)
  {
    return actualHeader.SequenceEqual(expectedHeader);
  }
}
