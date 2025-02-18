using System.Collections.Generic;
using System.Xml.Linq;

namespace GbxRemote.XmlRpc.Serialization.Models;

public class XmlRpcResponseArray : IXmlRpcResponseValue<List<object?>>
{
  public List<dynamic?>? Value { get; private set; }

  public bool IsValidType(string? typeId)
  {
    return typeId == "array";
  }

  public void DeserializeXml(XElement element)
  {
    Value = [];
    foreach (XElement member in element.Element(XNameConstants.Data).Elements())
    {
      XElement valueNode = (XElement)member.FirstNode;
      switch (valueNode.Name.LocalName)
      {
        case "array":
          XmlRpcResponseArray arraySerializer = new XmlRpcResponseArray();
          arraySerializer.DeserializeXml(valueNode);
          Value.Add(arraySerializer.Value);
          break;
        case "boolean":
          XmlRpcResponseBoolean boolSerializer = new XmlRpcResponseBoolean();
          boolSerializer.DeserializeXml(valueNode);
          Value.Add(boolSerializer.Value);
          break;
        case "double":
          XmlRpcResponseDouble doubleSerializer = new XmlRpcResponseDouble();
          doubleSerializer.DeserializeXml(valueNode);
          Value.Add(doubleSerializer.Value);
          break;
        case "i4":
        case "int":
          XmlRpcResponseInt32 intSerializer = new XmlRpcResponseInt32();
          intSerializer.DeserializeXml(valueNode);
          Value.Add(intSerializer.Value);
          break;
        case "string":
          XmlRpcResponseString stringSerializer = new XmlRpcResponseString();
          stringSerializer.DeserializeXml(valueNode);
          Value.Add(stringSerializer.Value);
          break;
        case "struct":
          XmlRpcResponseStruct structSerializer = new XmlRpcResponseStruct();
          structSerializer.DeserializeXml(valueNode);
          Value.Add(structSerializer.Value);
          break;
      }
    }
  }
}
