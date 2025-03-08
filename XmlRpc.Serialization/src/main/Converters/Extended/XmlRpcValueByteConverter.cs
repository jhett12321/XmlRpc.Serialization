namespace XmlRpc.Serialization.Converters.Extended;

internal sealed class XmlRpcValueByteConverter : XmlRpcValueConverter<byte>
{
  public static readonly XmlRpcValueByteConverter Converter = new XmlRpcValueByteConverter();
  public static readonly XmlRpcArrayConverter<byte> ArrayConverter = new XmlRpcArrayConverter<byte>(Converter);

  public override byte Deserialize(XmlRpcReader reader)
  {
    return (byte)reader.GetInt32();
  }

  public override void Serialize(XmlRpcWriter writer, byte value)
  {
    writer.WriteInt32Value(value);
  }
}
