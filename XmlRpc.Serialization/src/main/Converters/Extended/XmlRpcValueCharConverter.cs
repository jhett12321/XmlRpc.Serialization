using System.Linq;

namespace XmlRpc.Serialization.Converters.Extended;

internal sealed class XmlRpcValueCharConverter : XmlRpcValueConverter<char>
{
  public static readonly XmlRpcValueCharConverter Converter = new XmlRpcValueCharConverter();
  public static readonly XmlRpcArrayConverter<char> ArrayConverter = new XmlRpcArrayConverter<char>(Converter);

  public override char Deserialize(XmlRpcReader reader)
  {
    return reader.GetString().Single();
  }

  public override void Serialize(XmlRpcWriter writer, char value)
  {
    writer.WriteStringValue(new string([value]));
  }
}
