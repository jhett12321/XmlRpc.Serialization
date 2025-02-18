using System;
using System.Xml.Linq;

namespace GbxRemote.XmlRpc.Serialization.Models;

public sealed class XmlRpcResponseBoolean : IXmlRpcResponseValue<bool>
{
  public bool Value { get; private set; }

  public bool IsValidType(string? typeId)
  {
    return typeId == "boolean";
  }

  public void DeserializeXml(XElement element)
  {
    Value = element.Value switch
    {
      "1" => true,
      "0" => false,
      _ => throw new ArgumentOutOfRangeException(nameof(element.Value), element.Value, null),
    };
  }

  public static implicit operator bool(XmlRpcResponseBoolean xmlRpcResponseBoolean)
  {
    return xmlRpcResponseBoolean.Value;
  }
}
