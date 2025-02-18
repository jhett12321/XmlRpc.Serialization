using System.Xml.Linq;

namespace GbxRemote.XmlRpc.Serialization.Models;

public sealed class XmlRpcResponseString : IXmlRpcResponseValue<string>
{
  public string? Value { get; private set; }

  public bool IsValidType(string? typeId)
  {
    return typeId == "string";
  }

  public void DeserializeXml(XElement element)
  {
    Value = element.Value;
  }

  public static implicit operator string?(XmlRpcResponseString xmlRpcResponseString)
  {
    return xmlRpcResponseString.Value;
  }
}
