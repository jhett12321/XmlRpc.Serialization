using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace GbxRemote.XmlRpc.Models;

[XmlRoot("methodCall")]
public sealed class XmlRpcRequestMessage
{
  [XmlElement("methodName")]
  public required string MethodName { get; set; }

  [XmlArray("params")]
  [XmlArrayItem("param")]
  public List<XmlRpcRequestParameter>? Parameters { get; set; }

  private XmlRpcRequestMessage() {}

  [SetsRequiredMembers]
  public XmlRpcRequestMessage(string methodName, params object[]? parameters)
  {
    MethodName = methodName;
    if (parameters == null || parameters.Length == 0)
    {
      return;
    }

    Parameters = [];
    foreach (object parameter in parameters)
    {
      Parameters.Add(new XmlRpcRequestParameter(parameter));
    }
  }
}
