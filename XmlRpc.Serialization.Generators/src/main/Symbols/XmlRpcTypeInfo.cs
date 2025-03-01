using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace XmlRpc.Serialization.Generators.Symbols;

internal sealed record XmlRpcTypeInfo
{
  public INamedTypeSymbol Type { get; }

  public List<XmlRpcPropertyInfo> Properties { get; } = [];

  public List<XmlRpcMethodInfo> Methods { get; } = [];

  public XmlRpcTypeInfo(INamedTypeSymbol type)
  {
    Type = type;

    foreach (ISymbol symbol in type.GetMembers())
    {
      switch (symbol)
      {
        case IMethodSymbol methodSymbol:
          Methods.Add(new XmlRpcMethodInfo(methodSymbol));
          break;
        case IPropertySymbol propertySymbol:
          Properties.Add(new XmlRpcPropertyInfo(propertySymbol));
          break;
      }
    }
  }
}
