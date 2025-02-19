namespace GbxRemote.XmlRpc.Serialization.Converters
{
  internal sealed class XmlRpcValueStringConverter : XmlRpcValueConverter<string>
  {
    public static readonly XmlRpcValueStringConverter Instance = new XmlRpcValueStringConverter();

    public override string Deserialize(XmlRpcReader reader)
    {
      return reader.GetString();
    }

    public override void Serialize(XmlRpcWriter writer, string value)
    {
      writer.WriteStringValue(value);
    }
  }
}
