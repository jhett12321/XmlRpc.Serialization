using System;

namespace XmlRpc.Serialization.Converters;

internal sealed class XmlRpcValueDateTimeConverter : XmlRpcValueConverter<DateTime>
{
  public static readonly XmlRpcValueDateTimeConverter Converter = new XmlRpcValueDateTimeConverter();
  public static readonly XmlRpcArrayConverter<DateTime> ArrayConverter = new XmlRpcArrayConverter<DateTime>(Converter);

  public override DateTime Deserialize(XmlRpcReader reader)
  {
    return reader.GetDateTime();
  }

  public override void Serialize(XmlRpcWriter writer, DateTime value)
  {
    writer.WriteDateTimeValue(value);
  }
}
