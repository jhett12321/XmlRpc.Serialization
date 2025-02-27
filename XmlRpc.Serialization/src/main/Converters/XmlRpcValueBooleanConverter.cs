namespace XmlRpc.Serialization.Converters;

internal sealed class XmlRpcValueBooleanConverter : XmlRpcValueConverter<bool>
{
  public static readonly XmlRpcValueBooleanConverter Instance = new XmlRpcValueBooleanConverter();

  public override bool Deserialize(XmlRpcReader reader)
  {
    return reader.GetBoolean();
  }

  public override void Serialize(XmlRpcWriter writer, bool value)
  {
    writer.WriteBooleanValue(value);
  }
}