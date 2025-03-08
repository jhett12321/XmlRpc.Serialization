namespace XmlRpc.Serialization.Converters.Extended;

internal sealed class XmlRpcValueSingleConverter : XmlRpcValueConverter<float>
{
  public static readonly XmlRpcValueSingleConverter Converter = new XmlRpcValueSingleConverter();
  public static readonly XmlRpcArrayConverter<float> ArrayConverter = new XmlRpcArrayConverter<float>(Converter);

  public override float Deserialize(XmlRpcReader reader)
  {
    return (float)reader.GetDouble();
  }

  public override void Serialize(XmlRpcWriter writer, float value)
  {
    writer.WriteDoubleValue(value);
  }
}
