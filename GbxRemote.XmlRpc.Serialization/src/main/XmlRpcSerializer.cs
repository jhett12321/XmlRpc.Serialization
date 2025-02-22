using System.IO;
using GbxRemote.XmlRpc.Serialization.Converters;
using GbxRemote.XmlRpc.Serialization.Exceptions;
using GbxRemote.XmlRpc.Serialization.Models;

namespace GbxRemote.XmlRpc.Serialization;

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

        return retVal;
      }
      case XmlRpcTokenType.StartFault:
      {
        XmlRpcFaultResponse faultInfo = XmlRpcFaultResponseConverter.Instance.Deserialize(reader);
        reader.Read(XmlRpcTokenType.EndFault);
        reader.Read(XmlRpcTokenType.EndMethodResponse);

        throw new XmlRpcFaultException(faultInfo, $"Received fault response: ({faultInfo.FaultCode}) {faultInfo.Message}");
      }
      default:
        throw new XmlRpcSerializationException("Unexpected payload node: " + payloadToken);
    }
  }
}
