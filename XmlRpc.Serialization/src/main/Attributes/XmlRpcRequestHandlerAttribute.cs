using System;

namespace XmlRpc.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class XmlRpcRequestHandlerAttribute(Type type) : Attribute
{
  public Type Type { get; } = type;
}
