using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XmlRpc.Serialization.Generators.Extensions;

namespace XmlRpc.Serialization.Generators.Symbols;

internal sealed record XmlRpcContextInfo(ClassDeclarationSyntax Context, XmlRpcTypeInfo SerializedType)
{
  public ClassDeclarationSyntax Context { get; } = Context;

  public List<SyntaxNode> Parents { get; } = Context.GetParentNodes();

  public XmlRpcTypeInfo SerializedType { get; } = SerializedType;

  public string GetGeneratedFileName()
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (SyntaxNode node in Parents)
    {
      switch (node)
      {
        case BaseNamespaceDeclarationSyntax namespaceDeclaration:
          stringBuilder.Append(namespaceDeclaration.Name);
          stringBuilder.Append('.');
          break;
        case TypeDeclarationSyntax typeDeclaration:
          stringBuilder.Append(typeDeclaration.Identifier.Text);
          stringBuilder.Append('.');
          break;
        default:
          throw new Exception($"Unexpected SyntaxNode {node.GetType().FullName}");
      }
    }

    stringBuilder.Append(Context.Identifier.Text);
    stringBuilder.Append(".g.cs");
    return stringBuilder.ToString();
  }

  public List<XmlRpcTypeInfo> GetSerializedTypesFromProperties()
  {
    // ReSharper disable once UseObjectOrCollectionInitializer
    Dictionary<INamedTypeSymbol, XmlRpcTypeInfo> serializedTypes = new Dictionary<INamedTypeSymbol, XmlRpcTypeInfo>(SymbolEqualityComparer.Default);
    serializedTypes.Add(SerializedType.Type, SerializedType);

    CollectSerializedTypesFromProperties(serializedTypes, SerializedType);
    return serializedTypes.Values.ToList();
  }

  public List<XmlRpcTypeInfo> GetSerializedTypesFromMethods()
  {
    Dictionary<INamedTypeSymbol, XmlRpcTypeInfo> serializedTypes = new Dictionary<INamedTypeSymbol, XmlRpcTypeInfo>(SymbolEqualityComparer.Default);

    CollectionSerializedTypesFromMethods(serializedTypes, SerializedType);
    return serializedTypes.Values.ToList();
  }

  private void CollectSerializedTypesFromProperties(Dictionary<INamedTypeSymbol, XmlRpcTypeInfo> serializedTypes, XmlRpcTypeInfo typeInfo)
  {
    foreach (XmlRpcPropertyInfo property in typeInfo.Properties)
    {
      if (property.Ignored || property.Type is not INamedTypeSymbol type)
      {
        continue;
      }

      switch (property.XmlRpcSerializedType)
      {
        case XmlRpcSerializedType.UserArray:
          type = type.TypeArguments.OfType<INamedTypeSymbol>().First();
          break;
        case XmlRpcSerializedType.UserStruct:
          break;
        default:
          continue;
      }

      XmlRpcTypeInfo childTypeInfo = new XmlRpcTypeInfo(type);
      if (serializedTypes.ContainsKey(childTypeInfo.Type))
      {
        continue;
      }

      serializedTypes.Add(childTypeInfo.Type, childTypeInfo);
      CollectSerializedTypesFromProperties(serializedTypes, childTypeInfo);
    }
  }

  private void CollectionSerializedTypesFromMethods(Dictionary<INamedTypeSymbol, XmlRpcTypeInfo> serializedTypes, XmlRpcTypeInfo typeInfo)
  {
    foreach (XmlRpcMethodInfo method in typeInfo.Methods)
    {
      if (method.Ignored)
      {
        continue;
      }

      foreach (XmlRpcMethodParameterInfo parameter in method.Parameters)
      {
        if (parameter.Type is not INamedTypeSymbol type)
        {
          continue;
        }

        switch (parameter.XmlRpcSerializedType)
        {
          case XmlRpcSerializedType.UserArray:
            type = type.TypeArguments.OfType<INamedTypeSymbol>().First();
            break;
          case XmlRpcSerializedType.UserStruct:
            break;
          default:
            continue;
        }

        XmlRpcTypeInfo childTypeInfo = new XmlRpcTypeInfo(type);
        if (serializedTypes.ContainsKey(childTypeInfo.Type))
        {
          continue;
        }

        serializedTypes.Add(childTypeInfo.Type, childTypeInfo);
        CollectSerializedTypesFromProperties(serializedTypes, childTypeInfo);
      }
    }
  }
}
