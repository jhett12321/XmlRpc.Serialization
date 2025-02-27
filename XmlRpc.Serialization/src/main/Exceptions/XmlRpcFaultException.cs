using System;
using XmlRpc.Serialization.Models;

namespace XmlRpc.Serialization.Exceptions;

public class XmlRpcFaultException(XmlRpcFaultResponse faultInfo, string message) : Exception(message)
{
  public XmlRpcFaultResponse FaultInfo { get; } = faultInfo;
}
