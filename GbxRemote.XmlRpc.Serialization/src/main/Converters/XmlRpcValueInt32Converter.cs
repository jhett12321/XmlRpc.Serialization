namespace GbxRemote.XmlRpc.Serialization.Converters
{
  internal sealed class XmlRpcValueInt32Converter : XmlRpcValueConverter<int>
  {
    public static readonly XmlRpcValueInt32Converter Instance = new XmlRpcValueInt32Converter();

    public override int Deserialize(XmlRpcReader reader)
    {
      return reader.GetInt32();
    }

    public override void Serialize(XmlRpcWriter writer, int value)
    {
      writer.WriteInt32Value(value);
    }
  }
}
