using System.Collections.Generic;
using XmlRpc.Serialization.Exceptions;

namespace XmlRpc.Serialization.Converters;

public sealed class XmlRpcArrayConverter<T>(XmlRpcValueConverter<T> elementConverter) : XmlRpcValueConverter<List<T>>
{
  public override List<T> Deserialize(XmlRpcReader reader)
  {
    List<T> retVal = [];
    reader.ReadOrAdvance(XmlRpcTokenType.StartValue);

    reader.Read(XmlRpcTokenType.StartArray);
    reader.Read(XmlRpcTokenType.StartData);

    reader.Read();
    while (reader.TokenType is XmlRpcTokenType.StartValue)
    {
      retVal.Add(elementConverter.Deserialize(reader));
      reader.Read();
    }

    if (reader.TokenType != XmlRpcTokenType.EndData)
    {
      throw new XmlRpcSerializationException($"Expected node type '{XmlRpcTokenType.EndData}' in stream, but got '{reader.TokenType}'.");
    }

    reader.Read(XmlRpcTokenType.EndArray);
    reader.Read(XmlRpcTokenType.EndValue);

    return retVal;
  }

  public override void Serialize(XmlRpcWriter writer, List<T> value)
  {
    writer.Write(XmlRpcTokenType.StartValue);
    writer.Write(XmlRpcTokenType.StartArray);
    writer.Write(XmlRpcTokenType.StartData);

    foreach (T memberElement in value)
    {
      elementConverter.Serialize(writer, memberElement);
    }

    writer.Write(XmlRpcTokenType.EndData);
    writer.Write(XmlRpcTokenType.EndArray);
    writer.Write(XmlRpcTokenType.EndValue);
  }
}