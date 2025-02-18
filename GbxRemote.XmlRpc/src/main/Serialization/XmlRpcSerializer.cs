using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using GbxRemote.XmlRpc.Client;
using GbxRemote.XmlRpc.Serialization.Models;

namespace GbxRemote.XmlRpc.Serialization;

internal sealed class XmlRpcSerializer
{
  private static readonly Encoding Encoding = new UTF8Encoding(false);

  private readonly XmlSerializer requestSerializer = new XmlSerializer(typeof(XmlRpcRequestMessage));

  public Formatting Formatting { get; set; } = Formatting.None;

  public async Task<byte[]> SerializeAsync(XmlRpcRequestMessage request)
  {
    using MemoryStream requestStream = new MemoryStream();
    await using SimpleXmlTextWriter xmlWriter = new SimpleXmlTextWriter(requestStream, Encoding);
    xmlWriter.Formatting = Formatting;

    requestSerializer.Serialize(xmlWriter, request);
    return requestStream.ToArray();
  }

  public T Deserialize<T>(byte[] serializedXml) where T : IXmlRpcResponseValue, new()
  {
    try
    {
      string xml = Encoding.GetString(serializedXml);

      XDocument document = XDocument.Parse(xml);
      XElement docRoot = document.Root!;
      XElement faultRoot = docRoot.Element(XNameConstants.Fault);

      if (faultRoot != null)
      {
        XmlRpcFaultResponse faultInfo = DeserializeFaultResponse(faultRoot);
        throw new XmlRpcFaultException(faultInfo, $"Received fault response: ({faultInfo.FaultCode}) {faultInfo.Message}");
      }

      XElement? valueNode = (XElement?)docRoot.Element(XNameConstants.Params)?.Element(XNameConstants.Param)?.Element(XNameConstants.Value)?.FirstNode;

      T retVal = new T();

      if (valueNode == null || !retVal.IsValidType(valueNode.Name.LocalName))
      {
        throw new SerializationException($"Received invalid response type. Expected: '{retVal.GetType().Name}', got '{valueNode?.Name}'");
      }

      retVal.DeserializeXml(valueNode);
      return retVal;
    }
    catch (SerializationException)
    {
      throw;
    }
    catch (XmlRpcFaultException)
    {
      throw;
    }
    catch (Exception e)
    {
      throw new SerializationException("An exception occurred while deserializing the XML-RPC response.", e);
    }
  }

  private XmlRpcFaultResponse DeserializeFaultResponse(XElement faultRoot)
  {
    XElement? valueNode = (XElement?)faultRoot.Element(XNameConstants.Value)?.FirstNode;
    if (valueNode == null)
    {
      return new XmlRpcFaultResponse
      {
        Message = "Unknown fault",
        FaultCode = 0,
      };
    }

    XmlRpcResponseStruct responseStruct = new XmlRpcResponseStruct();
    responseStruct.DeserializeXml(valueNode);
    dynamic? response = responseStruct.Value;

    return new XmlRpcFaultResponse
    {
      Message = response?.faultString,
      FaultCode = response?.faultCode,
    };
  }
}
