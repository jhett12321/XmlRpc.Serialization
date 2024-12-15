using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using GbxRemote.XmlRpc.Models;

namespace GbxRemote.XmlRpc.Serialization;

internal sealed class XmlRpcSerializer
{
  private static readonly Encoding Encoding = new UTF8Encoding(false);

  private readonly XmlSerializer requestSerializer = new XmlSerializer(typeof(XmlRpcRequestMessage));
  private readonly ConcurrentDictionary<Type, XmlSerializer> responseSerializers = new ConcurrentDictionary<Type, XmlSerializer>();

  public Formatting Formatting { get; set; } = Formatting.None;

  public async Task SerializeAsync(Stream stream, XmlRpcRequestMessage request, uint requestId)
  {
    await using BinaryWriter streamWriter = new BinaryWriter(stream, Encoding, true);

    using MemoryStream requestStream = new MemoryStream();
    await using SimpleXmlTextWriter xmlWriter = new SimpleXmlTextWriter(requestStream, Encoding);
    xmlWriter.Formatting = Formatting;

    requestSerializer.Serialize(xmlWriter, request);

    streamWriter.Write((int)requestStream.Length);
    streamWriter.Write(requestId);

    requestStream.Position = 0;

    await requestStream.CopyToAsync(stream);
  }

  public T Deserialize<T>(Stream stream) where T : XmlRpcResponseMessage, new()
  {
    // TODO
    using BinaryReader reader = new BinaryReader(stream, Encoding, true);
    int size = reader.ReadInt32();

    return new T
    {
      ResponseId = reader.ReadUInt32(),
      Data = reader.ReadBytes(size),
    };
  }
}
