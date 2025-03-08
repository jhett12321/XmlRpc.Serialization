namespace XmlRpc.Serialization.Converters;

internal sealed class XmlRpcValueBooleanConverter : XmlRpcValueConverter<bool>
{
  public static readonly XmlRpcValueBooleanConverter Converter = new XmlRpcValueBooleanConverter();
  public static readonly XmlRpcArrayConverter<bool> ArrayConverter = new XmlRpcArrayConverter<bool>(Converter);

  public override bool Deserialize(XmlRpcReader reader)
  {
    return reader.GetBoolean();
  }

  public override void Serialize(XmlRpcWriter writer, bool value)
  {
    writer.WriteBooleanValue(value);
  }
}
