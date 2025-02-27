namespace XmlRpc.Serialization.Converters
{
  internal sealed class XmlRpcValueDoubleConverter : XmlRpcValueConverter<double>
  {
    public static readonly XmlRpcValueDoubleConverter Instance = new XmlRpcValueDoubleConverter();

    public override double Deserialize(XmlRpcReader reader)
    {
      return reader.GetDouble();
    }

    public override void Serialize(XmlRpcWriter writer, double value)
    {
      writer.WriteDoubleValue(value);
    }
  }
}
