namespace XmlRpc.Serialization.Converters.Extended;

internal sealed class XmlRpcValueSByteConverter : XmlRpcValueConverter<sbyte>
{
  public static readonly XmlRpcValueSByteConverter Converter = new XmlRpcValueSByteConverter();
  public static readonly XmlRpcArrayConverter<sbyte> ArrayConverter = new XmlRpcArrayConverter<sbyte>(Converter);

  public override sbyte Deserialize(XmlRpcReader reader)
  {
    return (sbyte)reader.GetInt32();
  }

  public override void Serialize(XmlRpcWriter writer, sbyte value)
  {
    writer.WriteInt32Value(value);
  }
}
