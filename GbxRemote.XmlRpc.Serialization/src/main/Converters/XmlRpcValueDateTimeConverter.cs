using System;

namespace GbxRemote.XmlRpc.Serialization.Converters
{
  internal sealed class XmlRpcValueDateTimeConverter : XmlRpcValueConverter<DateTime>
  {
    public static readonly XmlRpcValueDoubleConverter Instance = new XmlRpcValueDoubleConverter();

    public override DateTime Deserialize(XmlRpcReader reader)
    {
      return reader.GetDateTime();
    }

    public override void Serialize(XmlRpcWriter writer, DateTime value)
    {
      writer.WriteDateTimeValue(value);
    }
  }
}
