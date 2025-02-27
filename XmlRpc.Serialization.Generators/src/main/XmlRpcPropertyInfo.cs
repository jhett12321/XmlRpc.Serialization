using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace XmlRpc.Serialization.Generators;

internal sealed record XmlRpcPropertyInfo
{
  private const string PropertyNameAttributeClassName = "XmlRpcPropertyNameAttribute";
  private const string PropertyIgnoreAttributeClassName = "XmlRpcIgnore";

  public string PropertyName { get; }

  public ITypeSymbol Type { get; set; }

  public XmlRpcPropertyType XmlRpcPropertyType { get; }

  public string SerializedType { get; set; }

  public bool Ignored { get; set; }

  public XmlRpcPropertyInfo(IPropertySymbol property)
  {
    PropertyName = property.Name;
    SerializedType = property.Name;
    Type = property.Type;
    XmlRpcPropertyType = GetPropertyType(Type);

    ImmutableArray<AttributeData> attributes = property.GetAttributes();
    foreach (AttributeData attribute in attributes)
    {
      string? attributeType = attribute.AttributeClass?.Name;
      if (attributeType == PropertyNameAttributeClassName)
      {
        string? attributePropertyName = attribute.ConstructorArguments[0].Value?.ToString();
        if (attributePropertyName != null)
        {
          SerializedType = attributePropertyName;
        }
      }
      else if (attributeType == PropertyIgnoreAttributeClassName)
      {
        Ignored = true;
      }
    }
  }

  private XmlRpcPropertyType GetPropertyType(ITypeSymbol typeSymbol)
  {
    if (IsBuiltInType(typeSymbol.ToString()))
    {
      return XmlRpcPropertyType.BuiltIn;
    }

    if (typeSymbol is not INamedTypeSymbol type || type.IsAbstract)
    {
      return XmlRpcPropertyType.Unsupported;
    }

    if (type.ContainingAssembly.Name.StartsWith("System"))
    {
      if (type.Name != "List")
      {
        return XmlRpcPropertyType.Unsupported;
      }

      ITypeSymbol genericType = type.TypeArguments.First();
      if (IsBuiltInType(genericType.ToString()))
      {
        return XmlRpcPropertyType.BuiltIn;
      }

      if (!genericType.IsReferenceType || genericType.IsAbstract || genericType is not INamedTypeSymbol namedGenericType || namedGenericType.IsGenericType)
      {
        return XmlRpcPropertyType.Unsupported;
      }

      return XmlRpcPropertyType.UserArray;
    }

    if (!type.IsReferenceType || type.IsGenericType)
    {
      return XmlRpcPropertyType.Unsupported;
    }

    return XmlRpcPropertyType.UserStruct;
  }

  private bool IsBuiltInType(string typeName)
  {
    return typeName is "bool" or "int" or "double" or "string" or "System.DateTime" or "byte[]";
  }
}
