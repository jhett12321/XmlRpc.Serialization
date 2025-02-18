using System;
using System.Xml.Linq;

namespace GbxRemote.XmlRpc.Serialization.Models;

public class XmlRpcResponseBase64 : IXmlRpcResponseValue<byte[]>
{
  public byte[] Value { get; private set; }

  public bool IsValidType(string? typeId)
  {
    return typeId == "base64";
  }

  public void DeserializeXml(XElement element)
  {
    Value = Convert.FromBase64String(element.Value);
  }

  public static implicit operator byte[](XmlRpcResponseBase64 xmlRpcResponseBase64)
  {
    return xmlRpcResponseBase64.Value;
  }
}
