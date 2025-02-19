using System;
using GbxRemote.XmlRpc.Serialization.Models;

namespace GbxRemote.XmlRpc.Serialization.Exceptions;

public class XmlRpcFaultException(XmlRpcFaultResponse faultInfo, string message) : Exception(message)
{
  public XmlRpcFaultResponse FaultInfo { get; } = faultInfo;
}
