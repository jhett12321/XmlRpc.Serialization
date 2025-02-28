using System;

namespace XmlRpc.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class XmlRpcStructSerializableAttribute(Type type) : Attribute
{
  public Type Type { get; } = type;
}
