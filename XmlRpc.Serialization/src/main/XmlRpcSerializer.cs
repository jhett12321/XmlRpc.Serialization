using System.IO;
using XmlRpc.Serialization.Converters;
using XmlRpc.Serialization.Exceptions;

namespace XmlRpc.Serialization;

public static class XmlRpcSerializer
{
  public static byte[] SerializeRequest(XmlRpcRequestMessage requestMessage)
  {
    using MemoryStream requestStream = new MemoryStream();
    using (XmlRpcWriter writer = new XmlRpcWriter(requestStream))
    {
      writer.Write(XmlRpcTokenType.StartXmlDeclaration);
      writer.Write(XmlRpcTokenType.StartMethodCall);

      writer.WriteElement("methodName", requestMessage.MethodName);

      if (requestMessage.Parameters.Count > 0)
      {
        writer.Write(XmlRpcTokenType.StartParams);

        foreach (IXmlRpcRequestParameter parameter in requestMessage.Parameters)
        {
          writer.Write(XmlRpcTokenType.StartParam);
          parameter.Serialize(writer);
          writer.Write(XmlRpcTokenType.EndParam);
        }

        writer.Write(XmlRpcTokenType.EndParams);
      }

      writer.Write(XmlRpcTokenType.EndMethodCall);
      writer.Write(XmlRpcTokenType.EndXmlDeclaration);
    }

    return requestStream.ToArray();
  }

  public static void DeserializeRequest(byte[] serializedXml, IXmlRpcRequestHandler requestHandler)
  {
    using MemoryStream stream = new MemoryStream(serializedXml);
    using XmlRpcReader reader = new XmlRpcReader(stream);

    string? methodName = null;
    XmlRpcReader? paramsReader = null;

    reader.Read(XmlRpcTokenType.StartXmlDeclaration);
    reader.Read(XmlRpcTokenType.StartMethodCall);

    switch (reader.ReadNextToken())
    {
      case XmlRpcTokenType.StartMethodName:
        methodName = reader.ReadElementContentAsString();
        break;
      case XmlRpcTokenType.StartParams:
        paramsReader = reader.ReadSubtree(true);
        break;
      default:
        throw new XmlRpcSerializationException($"Expected node type 'StartMethodName' or 'StartParams' in stream, but got '{reader.TokenType}'.");
    }

    switch (reader.TokenType)
    {
      case XmlRpcTokenType.StartMethodName:
        methodName = reader.ReadElementContentAsString();
        break;
      case XmlRpcTokenType.StartParams:
        paramsReader = reader.ReadSubtree(false);
        break;
      default:
        break; // No method parameters
    }

    if (methodName == null)
    {
      throw new XmlRpcSerializationException("methodCall - methodName is not specified.");
    }

    paramsReader?.Read(XmlRpcTokenType.StartParams);
    requestHandler.HandleRequestMessage(methodName, paramsReader);
    paramsReader?.Dispose();

    reader.ValidateToken(XmlRpcTokenType.EndMethodCall);
    reader.Read();
  }

  public static byte[] SerializeResponse<T>(T response, XmlRpcValueConverter<T>? converter = null)
  {
    converter ??= XmlRpcConverterFactory.GetBuiltInValueConverter<T>();

    using MemoryStream requestStream = new MemoryStream();
    using (XmlRpcWriter writer = new XmlRpcWriter(requestStream))
    {
      writer.Write(XmlRpcTokenType.StartXmlDeclaration);
      writer.Write(XmlRpcTokenType.StartMethodResponse);

      if (response is XmlRpcFaultResponse faultResponse)
      {
        writer.Write(XmlRpcTokenType.StartFault);
        XmlRpcFaultResponseConverter.Converter.Serialize(writer, faultResponse);
        writer.Write(XmlRpcTokenType.EndFault);
      }
      else
      {
        writer.Write(XmlRpcTokenType.StartParams);
        writer.Write(XmlRpcTokenType.StartParam);
        converter.Serialize(writer, response);
        writer.Write(XmlRpcTokenType.EndParam);
        writer.Write(XmlRpcTokenType.EndParams);
      }

      writer.Write(XmlRpcTokenType.EndMethodResponse);
      writer.Write(XmlRpcTokenType.EndXmlDeclaration);
    }

    return requestStream.ToArray();
  }

  public static T DeserializeResponse<T>(byte[] serializedXml, XmlRpcValueConverter<T>? converter = null)
  {
    converter ??= XmlRpcConverterFactory.GetBuiltInValueConverter<T>();

    using MemoryStream stream = new MemoryStream(serializedXml);
    using XmlRpcReader reader = new XmlRpcReader(stream);

    reader.Read(XmlRpcTokenType.StartXmlDeclaration);
    reader.Read(XmlRpcTokenType.StartMethodResponse);
    XmlRpcTokenType payloadToken = reader.ReadNextToken();

    switch (payloadToken)
    {
      case XmlRpcTokenType.StartParams:
      {
        reader.Read(XmlRpcTokenType.StartParam);
        T retVal = converter.Deserialize(reader);
        reader.Read(XmlRpcTokenType.EndParam);
        reader.Read(XmlRpcTokenType.EndParams);
        reader.Read(XmlRpcTokenType.EndMethodResponse);
        reader.Read();

        return retVal;
      }
      case XmlRpcTokenType.StartFault:
      {
        XmlRpcFaultResponse faultInfo = XmlRpcFaultResponseConverter.Converter.Deserialize(reader);
        reader.Read(XmlRpcTokenType.EndFault);
        reader.Read(XmlRpcTokenType.EndMethodResponse);
        reader.Read();

        throw new XmlRpcFaultException(faultInfo, $"Received fault response: ({faultInfo.FaultCode}) {faultInfo.Message}");
      }
      default:
        throw new XmlRpcSerializationException("Unexpected payload node: " + payloadToken);
    }
  }
}
