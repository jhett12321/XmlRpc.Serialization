using System.Xml.Linq;

namespace GbxRemote.XmlRpc.Serialization.Models;

public class XmlRpcResponseRaw : IXmlRpcResponseValue<string>
{
  public string? Value { get; private set; }

  public bool IsValidType(string? typeId)
  {
    return true;
  }

  public void DeserializeXml(XElement element)
  {
    Value = element.ToString();
  }

  public static implicit operator string?(XmlRpcResponseRaw xmlRpcResponseRaw)
  {
    return xmlRpcResponseRaw.Value;
  }
}
