using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using XmlRpc.Serialization.Generators.Extensions;
using XmlRpc.Serialization.Generators.Symbols;

namespace XmlRpc.Serialization.Generators;

internal sealed class XmlRpcConverterGenerator(IndentedTextWriter textWriter, XmlRpcContextInfo contextInfo, ICollection<XmlRpcTypeInfo> serializableTypes)
{
  public void GenerateConverters()
  {
    foreach (XmlRpcTypeInfo typeInfo in serializableTypes)
    {
      textWriter.WriteLine($"public static readonly XmlRpcValueConverter<{typeInfo.Type}> {typeInfo.Type.Name} = new {typeInfo.Type.Name}ValueConverter();");
    }

    textWriter.WriteLineNoTabs(string.Empty);

    foreach (XmlRpcTypeInfo typeInfo in serializableTypes)
    {
      textWriter.WriteLine($"public static readonly XmlRpcValueConverter<List<{typeInfo.Type}>> {typeInfo.Type.Name}List = new XmlRpcArrayConverter<{typeInfo.Type.Name}>({typeInfo.Type.Name});");
    }

    textWriter.WriteLineNoTabs(string.Empty);

    foreach (XmlRpcTypeInfo typeInfo in serializableTypes)
    {
      GenerateConverterBody(typeInfo);
      textWriter.WriteLineNoTabs(string.Empty);
    }
  }

  private void GenerateConverterBody(XmlRpcTypeInfo typeInfo)
  {
    textWriter.BeginScope($"public sealed class {typeInfo.Type.Name}ValueConverter : XmlRpcStructConverter<{typeInfo.Type}>");
    {
      GenerateDeserializer(typeInfo, typeInfo.Properties);
      textWriter.WriteLineNoTabs(string.Empty);
      GenerateSerializer(typeInfo, typeInfo.Properties);
    }
    textWriter.EndScope();
  }

  private void GenerateDeserializer(XmlRpcTypeInfo typeInfo, List<XmlRpcPropertyInfo> properties)
  {
    textWriter.BeginScope($"protected override void PopulateStructMember({typeInfo.Type} value, string memberName, XmlRpcReader valueReader)");
    {
      textWriter.BeginScope("switch (memberName)");
      {
        foreach (XmlRpcPropertyInfo property in properties)
        {
          if (property.Ignored)
          {
            continue;
          }

          string converterCall = property.XmlRpcSerializedType switch
          {
            XmlRpcSerializedType.BuiltIn => $"XmlRpcConverterFactory.GetBuiltInValueConverter<{property.Type}>().Deserialize(valueReader);",
            XmlRpcSerializedType.UserArray => $"{contextInfo.Context.Identifier.Text}.{((INamedTypeSymbol)property.Type).TypeArguments[0].Name}List.Deserialize(valueReader);",
            XmlRpcSerializedType.UserStruct => $"{contextInfo.Context.Identifier.Text}.{property.Type.Name}.Deserialize(valueReader);",
            _ => throw new NotImplementedException($"Unsupported property type {property.Type.Name}"),
          };

          textWriter.BeginScope($"case \"{property.SerializedPropertyName}\":");
          {
            textWriter.WriteLine($"value.{property.PropertyName} = {converterCall}");
            textWriter.WriteLine("break;");
          }
          textWriter.EndScope();
        }
      }
      textWriter.EndScope();
    }
    textWriter.EndScope();
  }

  private void GenerateSerializer(XmlRpcTypeInfo typeInfo, List<XmlRpcPropertyInfo> properties)
  {
    textWriter.BeginScope($"protected override void WriteStructMembers(XmlRpcWriter writer, {typeInfo.Type} value)");
    {
      foreach (XmlRpcPropertyInfo property in properties)
      {
        if (property.Ignored)
        {
          continue;
        }

        string converterCall = property.XmlRpcSerializedType switch
        {
          XmlRpcSerializedType.BuiltIn => $"XmlRpcConverterFactory.GetBuiltInValueConverter<{property.Type}>().Serialize(writer, value.{property.PropertyName});",
          XmlRpcSerializedType.UserArray => $"{contextInfo.Context.Identifier.Text}.{((INamedTypeSymbol)property.Type).TypeArguments[0].Name}List.Serialize(writer, value.{property.PropertyName});",
          XmlRpcSerializedType.UserStruct => $"{contextInfo.Context.Identifier.Text}.{property.Type.Name}.Serialize(writer, value.{property.PropertyName});",
          _ => throw new NotImplementedException($"Unsupported property type {property.Type.Name}"),
        };

        textWriter.WriteLine($"writer.Write(XmlRpcTokenType.StartMember);");
        textWriter.WriteLine($"writer.WriteElement(\"name\", \"{property.SerializedPropertyName}\");");
        textWriter.WriteLine($"{converterCall}");
        textWriter.WriteLine($"writer.Write(XmlRpcTokenType.EndMember);");
      }
    }
    textWriter.EndScope();
  }
}
