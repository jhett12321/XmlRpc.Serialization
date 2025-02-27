using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace XmlRpc.Serialization.Generators;

public sealed record XmlRpcTypeInfo
{
  public INamedTypeSymbol Type { get; }

  public List<XmlRpcPropertyInfo> Properties { get; }

  public string ConverterName { get; }

  public XmlRpcTypeInfo(INamedTypeSymbol type)
  {
    Type = type;
    Properties = type.GetMembers().OfType<IPropertySymbol>()
      .Select(property => new XmlRpcPropertyInfo(property)).ToList();
  }
}
