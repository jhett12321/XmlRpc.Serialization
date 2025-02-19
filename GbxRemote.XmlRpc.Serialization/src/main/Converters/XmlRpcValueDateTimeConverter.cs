using System;

namespace GbxRemote.XmlRpc.Serialization.Converters
{
  internal sealed class XmlRpcValueDateTimeConverter : XmlRpcValueConverter<DateTime>
  {
    public static readonly XmlRpcValueDateTimeConverter Instance = new XmlRpcValueDateTimeConverter();

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
