namespace XmlRpc.Serialization.Converters;

internal sealed class XmlRpcValueInt32Converter : XmlRpcValueConverter<int>
{
  public static readonly XmlRpcValueInt32Converter Converter = new XmlRpcValueInt32Converter();
  public static readonly XmlRpcArrayConverter<int> ArrayConverter = new XmlRpcArrayConverter<int>(Converter);

  public override int Deserialize(XmlRpcReader reader)
  {
    return reader.GetInt32();
  }

  public override void Serialize(XmlRpcWriter writer, int value)
  {
    writer.WriteInt32Value(value);
  }
}
