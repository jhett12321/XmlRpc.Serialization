using System.Xml.Serialization;

namespace GbxRemote.XmlRpc.Serialization.Models;

public sealed class XmlRpcRequestParameter
{
  [XmlElement("value")]
  public object? Value { get; set; }

  private XmlRpcRequestParameter() {}

  public XmlRpcRequestParameter(object value)
  {
    Value = value;
  }
}
