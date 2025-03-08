namespace XmlRpc.Serialization.Converters;

internal sealed class XmlRpcValueDoubleConverter : XmlRpcValueConverter<double>
{
  public static readonly XmlRpcValueDoubleConverter Converter = new XmlRpcValueDoubleConverter();
  public static readonly XmlRpcArrayConverter<double> ArrayConverter = new XmlRpcArrayConverter<double>(XmlRpcValueDoubleConverter.Converter);

  public override double Deserialize(XmlRpcReader reader)
  {
    return reader.GetDouble();
  }

  public override void Serialize(XmlRpcWriter writer, double value)
  {
    writer.WriteDoubleValue(value);
  }
}
