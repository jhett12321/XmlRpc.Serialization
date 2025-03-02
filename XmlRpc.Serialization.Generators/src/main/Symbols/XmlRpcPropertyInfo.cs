using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using XmlRpc.Serialization.Generators.Extensions;

namespace XmlRpc.Serialization.Generators.Symbols;

internal sealed record XmlRpcPropertyInfo
{
  private const string PropertyNameAttributeClassName = "XmlRpcPropertyNameAttribute";
  private const string PropertyIgnoreAttributeClassName = "XmlRpcIgnore";

  public string PropertyName { get; }

  public ITypeSymbol Type { get; }

  public XmlRpcSerializedType XmlRpcSerializedType { get; }

  public string SerializedPropertyName { get; }

  public bool Ignored { get; }

  public XmlRpcPropertyInfo(IPropertySymbol property)
  {
    PropertyName = property.Name;
    SerializedPropertyName = property.Name;
    Type = property.Type;
    XmlRpcSerializedType = Type.GetSerializedType();

    ImmutableArray<AttributeData> attributes = property.GetAttributes();
    foreach (AttributeData attribute in attributes)
    {
      string? attributeType = attribute.AttributeClass?.Name;
      switch (attributeType)
      {
        case PropertyIgnoreAttributeClassName:
          Ignored = true;
          break;
        case PropertyNameAttributeClassName:
        {
          string? attributePropertyName = attribute.ConstructorArguments[0].Value?.ToString();
          if (attributePropertyName != null)
          {
            SerializedPropertyName = attributePropertyName;
          }

          break;
        }
      }
    }
  }
}
