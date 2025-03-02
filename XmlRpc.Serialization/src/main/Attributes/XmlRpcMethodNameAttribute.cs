using System;

namespace XmlRpc.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class XmlRpcMethodNameAttribute(string name) : Attribute
{
  public string Name { get; } = name;
}
