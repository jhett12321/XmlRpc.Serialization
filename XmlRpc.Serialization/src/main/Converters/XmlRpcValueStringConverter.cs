namespace XmlRpc.Serialization.Converters;

internal sealed class XmlRpcValueStringConverter : XmlRpcValueConverter<string>
{
  public static readonly XmlRpcValueStringConverter Converter = new XmlRpcValueStringConverter();
  public static readonly XmlRpcArrayConverter<string> ArrayConverter = new XmlRpcArrayConverter<string>(Converter);

  public override string Deserialize(XmlRpcReader reader)
  {
    return reader.GetString();
  }

  public override void Serialize(XmlRpcWriter writer, string value)
  {
    writer.WriteStringValue(value);
  }
}
