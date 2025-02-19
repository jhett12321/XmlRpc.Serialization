using System.Collections.Generic;

namespace GbxRemote.XmlRpc.Serialization.Models;

public sealed class XmlRpcRequestMessage
{
  public string MethodName { get; }

  public List<IXmlRpcRequestParameter> Parameters { get; } = [];

  public XmlRpcRequestMessage(string methodName)
  {
    MethodName = methodName;
  }

  public XmlRpcRequestMessage Param<T>(XmlRpcRequestParameter<T> parameter)
  {
    Parameters.Add(parameter);
    return this;
  }
}
