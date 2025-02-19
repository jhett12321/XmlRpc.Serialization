using System.IO;
using GbxRemote.XmlRpc.Serialization.Converters;
using GbxRemote.XmlRpc.Serialization.Exceptions;
using GbxRemote.XmlRpc.Serialization.Models;

namespace GbxRemote.XmlRpc.Serialization;

public static class XmlRpcSerializer
{
  public static byte[] Serialize(XmlRpcRequestMessage requestMessage)
  {
    using MemoryStream requestStream = new MemoryStream();
    using (XmlRpcWriter writer = new XmlRpcWriter(requestStream))
    {
      writer.Write(XmlRpcTokenType.StartXmlDeclaration);
      writer.Write(XmlRpcTokenType.StartPayload);

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

      writer.Write(XmlRpcTokenType.EndPayload);
      writer.Write(XmlRpcTokenType.EndXmlDeclaration);
    }

    return requestStream.ToArray();
  }

  public static T Deserialize<T>(byte[] serializedXml, XmlRpcValueConverter<T>? converter = null)
  {
    using MemoryStream stream = new MemoryStream(serializedXml);
    using XmlRpcReader reader = new XmlRpcReader(stream);

    converter ??= XmlRpcConverterFactory.GetBuiltInValueConverter<T>();

    reader.Read(XmlRpcTokenType.StartXmlDeclaration);
    reader.Read(XmlRpcTokenType.StartPayload);
    XmlRpcTokenType payloadToken = reader.ReadNextToken();

    switch (payloadToken)
    {
      case XmlRpcTokenType.StartParams:
      {
        reader.Read(XmlRpcTokenType.StartParam);
        T retVal = converter.Deserialize(reader);
        reader.Read(XmlRpcTokenType.EndParam);
        reader.Read(XmlRpcTokenType.EndParams);
        reader.Read(XmlRpcTokenType.EndPayload);

        return retVal;
      }
      case XmlRpcTokenType.StartFault:
      {
        XmlRpcFaultResponse faultInfo = XmlRpcFaultResponseConverter.Instance.Deserialize(reader);
        reader.Read(XmlRpcTokenType.EndFault);
        reader.Read(XmlRpcTokenType.EndPayload);

        throw new XmlRpcFaultException(faultInfo, $"Received fault response: ({faultInfo.FaultCode}) {faultInfo.Message}");
      }
      default:
        throw new XmlRpcSerializationException("Unexpected payload node: " + payloadToken);
    }
  }
}
