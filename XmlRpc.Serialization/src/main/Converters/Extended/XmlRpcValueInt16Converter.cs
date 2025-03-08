namespace XmlRpc.Serialization.Converters.Extended;

internal sealed class XmlRpcValueInt16Converter : XmlRpcValueConverter<short>
{
  public static readonly XmlRpcValueInt16Converter Converter = new XmlRpcValueInt16Converter();
  public static readonly XmlRpcArrayConverter<short> ArrayConverter = new XmlRpcArrayConverter<short>(Converter);

  public override short Deserialize(XmlRpcReader reader)
  {
    return (short)reader.GetInt32();
  }

  public override void Serialize(XmlRpcWriter writer, short value)
  {
    writer.WriteInt32Value(value);
  }
}
