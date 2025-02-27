using System;

namespace XmlRpc.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class XmlRpcPropertyNameAttribute(string name) : Attribute
{
  public string Name { get; } = name;
}
