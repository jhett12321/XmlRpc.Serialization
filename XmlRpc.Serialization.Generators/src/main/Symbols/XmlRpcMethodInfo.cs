using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace XmlRpc.Serialization.Generators.Symbols;

internal sealed record XmlRpcMethodInfo
{
  private const string MethodNameAttributeClassName = "XmlRpcMethodNameAttribute";

  public string MethodName { get; }

  public string SerializedMethodName { get; }

  public List<XmlRpcMethodParameterInfo> Parameters { get; } = [];

  public bool Ignored { get; }

  public XmlRpcMethodInfo(IMethodSymbol method)
  {
    MethodName = method.Name;
    SerializedMethodName = method.Name;
    Ignored = true;

    ImmutableArray<AttributeData> attributes = method.GetAttributes();
    foreach (AttributeData attribute in attributes)
    {
      string? attributeType = attribute.AttributeClass?.Name;
      if (attributeType == MethodNameAttributeClassName)
      {
        string? attributeMethodName = attribute.ConstructorArguments[0].Value?.ToString();
        if (attributeMethodName != null)
        {
          SerializedMethodName = attributeMethodName;
          Ignored = false;
        }
      }
    }

    if (Ignored)
    {
      return;
    }

    foreach (IParameterSymbol parameter in method.Parameters)
    {
      Parameters.Add(new XmlRpcMethodParameterInfo(parameter));
    }
  }
}
