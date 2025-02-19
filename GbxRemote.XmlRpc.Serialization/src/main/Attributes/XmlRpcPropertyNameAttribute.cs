using System;

namespace GbxRemote.XmlRpc.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class XmlRpcPropertyNameAttribute(string name) : Attribute
{
  public string Name { get; } = name;
}
