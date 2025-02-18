using System.Xml.Linq;

namespace GbxRemote.XmlRpc.Serialization.Models;

public sealed class XmlRpcResponseInt32 : IXmlRpcResponseValue<int>
{
  public int Value { get; private set; }

  public bool IsValidType(string? typeId)
  {
    return typeId is "i4" or "int";
  }

  public void DeserializeXml(XElement element)
  {
    Value = int.Parse(element.Value);
  }

  public static implicit operator int(XmlRpcResponseInt32 xmlRpcResponseInt32)
  {
    return xmlRpcResponseInt32.Value;
  }
}
