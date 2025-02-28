using System.Linq;
using Microsoft.CodeAnalysis;
using XmlRpc.Serialization.Generators.Symbols;

namespace XmlRpc.Serialization.Generators.Extensions;

internal static class TypeSymbolExtensions
{
  public static XmlRpcSerializedType GetSerializedType(this ITypeSymbol typeSymbol)
  {
    if (IsBuiltInType(typeSymbol.ToString()))
    {
      return XmlRpcSerializedType.BuiltIn;
    }

    if (typeSymbol is not INamedTypeSymbol type || type.IsAbstract)
    {
      return XmlRpcSerializedType.Unsupported;
    }

    if (type.ContainingAssembly.Name.StartsWith("System"))
    {
      if (type.Name != "List")
      {
        return XmlRpcSerializedType.Unsupported;
      }

      ITypeSymbol genericType = type.TypeArguments.First();
      if (IsBuiltInType(genericType.ToString()))
      {
        return XmlRpcSerializedType.BuiltIn;
      }

      if (!genericType.IsReferenceType || genericType.IsAbstract || genericType is not INamedTypeSymbol namedGenericType || namedGenericType.IsGenericType)
      {
        return XmlRpcSerializedType.Unsupported;
      }

      return XmlRpcSerializedType.UserArray;
    }

    if (!type.IsReferenceType || type.IsGenericType)
    {
      return XmlRpcSerializedType.Unsupported;
    }

    return XmlRpcSerializedType.UserStruct;
  }

  private static bool IsBuiltInType(string typeName)
  {
    return typeName is "bool" or "int" or "double" or "string" or "System.DateTime" or "byte[]";
  }
}
