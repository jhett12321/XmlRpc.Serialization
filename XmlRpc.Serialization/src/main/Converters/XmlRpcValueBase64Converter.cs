namespace XmlRpc.Serialization.Converters;

internal sealed class XmlRpcValueBase64Converter : XmlRpcValueConverter<byte[]>
{
  public static readonly XmlRpcValueBase64Converter Converter = new XmlRpcValueBase64Converter();
  public static readonly XmlRpcArrayConverter<byte[]> ArrayConverter = new XmlRpcArrayConverter<byte[]>(Converter);

  public override byte[] Deserialize(XmlRpcReader reader)
  {
    return reader.GetBase64Bytes();
  }

  public override void Serialize(XmlRpcWriter writer, byte[] value)
  {
    writer.WriteBase64BytesValue(value);
  }
}
