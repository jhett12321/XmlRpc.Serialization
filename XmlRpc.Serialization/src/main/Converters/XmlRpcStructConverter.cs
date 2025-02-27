using XmlRpc.Serialization.Exceptions;

namespace XmlRpc.Serialization.Converters;

public abstract class XmlRpcStructConverter<T> : XmlRpcValueConverter<T> where T : new()
{
  public override T Deserialize(XmlRpcReader reader)
  {
    T retVal = new T();
    if (reader.TokenType != XmlRpcTokenType.StartValue)
    {
      reader.Read(XmlRpcTokenType.StartValue);
    }

    reader.Read(XmlRpcTokenType.StartStruct);

    XmlRpcTokenType currentToken;
    while ((currentToken = reader.ReadNextToken()) == XmlRpcTokenType.StartMember)
    {
      reader.ReadStructMember((memberName, valueReader) =>
      {
        PopulateStructMember(retVal, memberName, valueReader);
      });
    }

    if (currentToken != XmlRpcTokenType.EndStruct)
    {
      throw new XmlRpcSerializationException($"Unexpected token '{currentToken}', expected '{XmlRpcTokenType.EndStruct}'.");
    }

    reader.Read(XmlRpcTokenType.EndValue);

    return retVal;
  }

  public override void Serialize(XmlRpcWriter writer, T value)
  {
    writer.Write(XmlRpcTokenType.StartValue);
    writer.Write(XmlRpcTokenType.StartStruct);

    WriteStructMembers(writer, value);

    writer.Write(XmlRpcTokenType.EndStruct);
    writer.Write(XmlRpcTokenType.EndValue);
  }

  protected abstract void PopulateStructMember(T value, string memberName, XmlRpcReader valueReader);

  protected abstract void WriteStructMembers(XmlRpcWriter writer, T value);
}