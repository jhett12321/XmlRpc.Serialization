using System;

namespace GbxRemote.XmlRpc.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class XmlRpcStructSerializableAttribute(Type type) : Attribute
{
  public Type Type { get; } = type;
}
