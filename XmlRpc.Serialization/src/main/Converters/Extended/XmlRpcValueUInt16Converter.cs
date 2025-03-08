namespace XmlRpc.Serialization.Converters.Extended;

internal sealed class XmlRpcValueUInt16Converter : XmlRpcValueConverter<ushort>
{
  public static readonly XmlRpcValueUInt16Converter Converter = new XmlRpcValueUInt16Converter();
  public static readonly XmlRpcArrayConverter<ushort> ArrayConverter = new XmlRpcArrayConverter<ushort>(Converter);

  public override ushort Deserialize(XmlRpcReader reader)
  {
    return (ushort)reader.GetInt32();
  }

  public override void Serialize(XmlRpcWriter writer, ushort value)
  {
    writer.WriteInt32Value(value);
  }
}
