using System.Xml.Linq;

namespace GbxRemote.XmlRpc.Serialization.Models;

public sealed class XmlRpcResponseDouble : IXmlRpcResponseValue<double>
{
  public double Value { get; private set; }

  public bool IsValidType(string? typeId)
  {
    return typeId == "double";
  }

  public void DeserializeXml(XElement element)
  {
    Value = double.Parse(element.Value);
  }

  public static implicit operator double(XmlRpcResponseDouble xmlRpcResponseDouble)
  {
    return xmlRpcResponseDouble.Value;
  }
}
