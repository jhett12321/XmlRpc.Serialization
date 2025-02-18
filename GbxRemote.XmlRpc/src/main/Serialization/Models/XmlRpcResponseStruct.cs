using System.Collections.Generic;
using System.Dynamic;
using System.Xml.Linq;

namespace GbxRemote.XmlRpc.Serialization.Models;

public class XmlRpcResponseStruct : IXmlRpcResponseValue<object>
{
  public dynamic? Value { get; private set; }

  public bool IsValidType(string? typeId)
  {
    return typeId == "struct";
  }

  public void DeserializeXml(XElement element)
  {
    Value = new ExpandoObject();
    IDictionary<string, object> dynamicProperties = Value;

    foreach (XElement member in element.Elements())
    {
      string memberName = member.Element(XNameConstants.Name).Value;
      XElement valueNode = (XElement)member.Element(XNameConstants.Value).FirstNode;

      switch (valueNode.Name.LocalName)
      {
        case "array":
          XmlRpcResponseArray arraySerializer = new XmlRpcResponseArray();
          arraySerializer.DeserializeXml(valueNode);
          dynamicProperties.Add(memberName, arraySerializer.Value);
          break;
        case "boolean":
          XmlRpcResponseBoolean boolSerializer = new XmlRpcResponseBoolean();
          boolSerializer.DeserializeXml(valueNode);
          dynamicProperties.Add(memberName, boolSerializer.Value);
          break;
        case "double":
          XmlRpcResponseDouble doubleSerializer = new XmlRpcResponseDouble();
          doubleSerializer.DeserializeXml(valueNode);
          dynamicProperties.Add(memberName, doubleSerializer.Value);
          break;
        case "i4":
        case "int":
          XmlRpcResponseInt32 intSerializer = new XmlRpcResponseInt32();
          intSerializer.DeserializeXml(valueNode);
          dynamicProperties.Add(memberName, intSerializer.Value);
          break;
        case "string":
          XmlRpcResponseString stringSerializer = new XmlRpcResponseString();
          stringSerializer.DeserializeXml(valueNode);
          dynamicProperties.Add(memberName, stringSerializer.Value);
          break;
        case "struct":
          XmlRpcResponseStruct structSerializer = new XmlRpcResponseStruct();
          structSerializer.DeserializeXml(valueNode);
          dynamicProperties.Add(memberName, structSerializer.Value);
          break;
      }
    }
  }

  public static implicit operator ExpandoObject?(XmlRpcResponseStruct value)
  {
    return value.Value;
  }
}
