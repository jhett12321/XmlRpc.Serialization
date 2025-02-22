using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GbxRemote.XmlRpc.Serialization.Generators;

[Generator]
internal class StructConverterGenerator : IIncrementalGenerator
{
  private const string ClassAttributeTypeName = "GbxRemote.XmlRpc.Serialization.Attributes.XmlRpcStructSerializableAttribute";
  private const string ClassAttributeClassName = "XmlRpcStructSerializableAttribute";

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    IncrementalValuesProvider<XmlRpcContextInfo?> classDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName<XmlRpcContextInfo?>(
        ClassAttributeTypeName,
        (node, _) => node is ClassDeclarationSyntax,
        (ctx, _) => GetSemanticTargetForGeneration(ctx))
      .Where(info => info is not null);

    IncrementalValueProvider<(Compilation, ImmutableArray<XmlRpcContextInfo?>)> compilation = context.CompilationProvider.Combine(classDeclarations.Collect());
    context.RegisterSourceOutput(compilation, (spc, source) => Execute(source.Item1, source.Item2!, spc));
  }

  private void Execute(Compilation compilation, ImmutableArray<XmlRpcContextInfo> classes, SourceProductionContext context)
  {
    foreach (XmlRpcContextInfo classInfo in classes)
    {
      if (classInfo.SerializedType.Type.TypeKind != TypeKind.Class)
      {
        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
          "XR001",
          "Invalid converter type",
          "Class {0} is not a class type. XML-RPC struct types must be a class.",
          "generation",
          DiagnosticSeverity.Error,
          true), classInfo.ClassDeclaration.GetLocation(), classInfo.ClassDeclaration.Identifier.Text));
        return;
      }

      context.AddSource(GetFileName(classInfo), GenerateContextClass(classInfo));
    }
  }

  private XmlRpcContextInfo? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
  {
    if (context.TargetNode is not ClassDeclarationSyntax classDeclaration)
    {
      return null;
    }

    AttributeData? attribute = context.Attributes.FirstOrDefault(a => a.AttributeClass?.Name == ClassAttributeClassName);
    if (attribute == null)
    {
      return null;
    }

    List<SyntaxNode> parentNodes = [];
    SyntaxNode? parent = classDeclaration.Parent;

    while (parent != null)
    {
      if (parent is not CompilationUnitSyntax)
      {
        parentNodes.Add(parent);
      }

      parent = parent.Parent;
    }

    parentNodes.Reverse();

    return new XmlRpcContextInfo(classDeclaration, attribute, parentNodes);
  }

  private string GetFileName(XmlRpcContextInfo classInfo)
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (SyntaxNode node in classInfo.Parents)
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
          throw new Exception($"Unexpected SyntaxNode {classInfo.GetType().FullName}");
      }
    }

    stringBuilder.Append(classInfo.ClassDeclaration.Identifier.Text);
    stringBuilder.Append(".g.cs");
    return stringBuilder.ToString();
  }

  private string GenerateContextClass(XmlRpcContextInfo classInfo)
  {
    StringBuilder stringBuilder = new StringBuilder();

    Dictionary<INamedTypeSymbol, XmlRpcTypeInfo> serializedTypes = new Dictionary<INamedTypeSymbol, XmlRpcTypeInfo>();
    serializedTypes.Add(classInfo.SerializedType.Type, classInfo.SerializedType);
    CollectSerializedTypes(serializedTypes, classInfo.SerializedType);

    string baseIndent = new string(' ', classInfo.Parents.Count * 2);
    GenerateClassHeader(stringBuilder, baseIndent, classInfo);
    GenerateConverters(stringBuilder, baseIndent, classInfo, serializedTypes.Values);
    GenerateClassFooter(stringBuilder, baseIndent, classInfo);

    return stringBuilder.ToString();
  }

  private void CollectSerializedTypes(Dictionary<INamedTypeSymbol, XmlRpcTypeInfo> serializedTypes, XmlRpcTypeInfo typeInfo)
  {
    foreach (XmlRpcPropertyInfo property in typeInfo.Properties)
    {
      if (property.Ignored || property.Type is not INamedTypeSymbol type)
      {
        continue;
      }

      switch (property.XmlRpcPropertyType)
      {
        case XmlRpcPropertyType.UserArray:
          type = type.TypeArguments.OfType<INamedTypeSymbol>().First();
          break;
        case XmlRpcPropertyType.UserStruct:
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
      CollectSerializedTypes(serializedTypes, childTypeInfo);
    }
  }

  private void GenerateClassHeader(StringBuilder stringBuilder, string baseIndent, XmlRpcContextInfo classInfo)
  {
    stringBuilder.AppendLine("// <auto-generated/>");
    stringBuilder.AppendLine("using System;");
    stringBuilder.AppendLine("using GbxRemote.XmlRpc.Serialization;");
    stringBuilder.AppendLine("using GbxRemote.XmlRpc.Serialization.Converters;");
    stringBuilder.AppendLine();

    for (int i = 0; i < classInfo.Parents.Count; i++)
    {
      SyntaxNode syntaxNode = classInfo.Parents[i];
      string indent = new string(' ', i * 2);

      switch (syntaxNode)
      {
        case BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax:
          stringBuilder.AppendLine($"namespace {namespaceDeclarationSyntax.Name}");
          stringBuilder.AppendLine("{");
          break;
        case TypeDeclarationSyntax typeDeclarationSyntax:
          WriteTypeDeclarationOpen(stringBuilder, indent, typeDeclarationSyntax);
          break;
        default:
          throw new Exception($"Unexpected SyntaxNode {classInfo.GetType().FullName}");
      }
    }

    WriteTypeDeclarationOpen(stringBuilder, baseIndent, classInfo.ClassDeclaration);
  }

  private void GenerateConverters(StringBuilder stringBuilder, string baseIndent, XmlRpcContextInfo contextInfo, ICollection<XmlRpcTypeInfo> serializableTypes)
  {
    foreach (XmlRpcTypeInfo typeInfo in serializableTypes)
    {
      stringBuilder.AppendLine($"{baseIndent}  public static readonly XmlRpcValueConverter<{typeInfo.Type}> {typeInfo.Type.Name} = new {typeInfo.Type.Name}ValueConverter();");
    }

    stringBuilder.AppendLine();

    foreach (XmlRpcTypeInfo typeInfo in serializableTypes)
    {
      stringBuilder.AppendLine($"{baseIndent}  public static readonly XmlRpcValueConverter<List<{typeInfo.Type}>> {typeInfo.Type.Name}List = new XmlRpcArrayConverter<{typeInfo.Type.Name}>({typeInfo.Type.Name});");
    }

    stringBuilder.AppendLine();

    foreach (XmlRpcTypeInfo typeInfo in serializableTypes)
    {
      GenerateConverterBody(stringBuilder, baseIndent, contextInfo, typeInfo);
      stringBuilder.AppendLine();
    }
  }

  private void GenerateConverterBody(StringBuilder stringBuilder, string baseIndent, XmlRpcContextInfo contextInfo, XmlRpcTypeInfo typeInfo)
  {
    stringBuilder.AppendLine($"{baseIndent}  public sealed class {typeInfo.Type.Name}ValueConverter : XmlRpcStructConverter<{typeInfo.Type}>");
    stringBuilder.AppendLine($"{baseIndent}  {{");

    string childIndent = baseIndent + "    ";

    GenerateDeserializer(stringBuilder, childIndent, contextInfo, typeInfo, typeInfo.Properties);
    stringBuilder.AppendLine();
    GenerateSerializer(stringBuilder, childIndent, contextInfo, typeInfo, typeInfo.Properties);

    WriteTypeDeclarationClose(stringBuilder, $"{baseIndent}  ");
  }

  private void GenerateDeserializer(StringBuilder stringBuilder, string childIndent, XmlRpcContextInfo contextInfo, XmlRpcTypeInfo typeInfo, List<XmlRpcPropertyInfo> properties)
  {
    stringBuilder.AppendLine($$"""
                               {{childIndent}}protected override void PopulateStructMember({{typeInfo.Type}} value, string memberName, XmlRpcReader valueReader)
                               {{childIndent}}{
                               {{childIndent}}  switch (memberName)
                               {{childIndent}}  {
                               {{GeneratePropertyAssignments(childIndent, contextInfo, properties)}}
                               {{childIndent}}  }
                               {{childIndent}}}
                               """);
  }

  private void GenerateSerializer(StringBuilder stringBuilder, string childIndent, XmlRpcContextInfo contextInfo, XmlRpcTypeInfo typeInfo, List<XmlRpcPropertyInfo> properties)
  {
    stringBuilder.AppendLine($$"""
                               {{childIndent}}protected override void WriteStructMembers(XmlRpcWriter writer, {{typeInfo.Type}} value)
                               {{childIndent}}{
                               {{GeneratePropertyWriters(childIndent, contextInfo, properties)}}
                               {{childIndent}}}
                               """);
  }

  private string GeneratePropertyAssignments(string childIndent, XmlRpcContextInfo contextInfo, List<XmlRpcPropertyInfo> properties)
  {
    StringBuilder stringBuilder = new StringBuilder();

    foreach (XmlRpcPropertyInfo property in properties)
    {
      if (property.Ignored)
      {
        continue;
      }

      string converterCall = property.XmlRpcPropertyType switch
      {
        XmlRpcPropertyType.BuiltIn => $"XmlRpcConverterFactory.GetBuiltInValueConverter<{property.Type}>().Deserialize(valueReader);",
        XmlRpcPropertyType.UserArray => $"{contextInfo.ClassDeclaration.Identifier.Text}.{((INamedTypeSymbol)property.Type).TypeArguments[0].Name}List.Deserialize(valueReader);",
        XmlRpcPropertyType.UserStruct => $"{contextInfo.ClassDeclaration.Identifier.Text}.{property.Type.Name}.Deserialize(valueReader);",
        _ => throw new NotImplementedException($"Unsupported property type {property.Type.Name}"),
      };

      stringBuilder.AppendLine($"""
                                 {childIndent}    case "{property.SerializedType}":
                                 {childIndent}      value.{property.PropertyName} = {converterCall}
                                 {childIndent}      break;
                                 """);
    }

    return stringBuilder.ToString();
  }

  private string GeneratePropertyWriters(string childIndent, XmlRpcContextInfo contextInfo, List<XmlRpcPropertyInfo> properties)
  {
    StringBuilder stringBuilder = new StringBuilder();

    foreach (XmlRpcPropertyInfo property in properties)
    {
      if (property.Ignored)
      {
        continue;
      }

      string converterCall = property.XmlRpcPropertyType switch
      {
        XmlRpcPropertyType.BuiltIn => $"XmlRpcConverterFactory.GetBuiltInValueConverter<{property.Type}>().Serialize(writer, value.{property.PropertyName});",
        XmlRpcPropertyType.UserArray => $"{contextInfo.ClassDeclaration.Identifier.Text}.{((INamedTypeSymbol)property.Type).TypeArguments[0].Name}List.Serialize(writer, value.{property.PropertyName});",
        XmlRpcPropertyType.UserStruct => $"{contextInfo.ClassDeclaration.Identifier.Text}.{property.Type.Name}.Serialize(writer, value.{property.PropertyName});",
        _ => throw new NotImplementedException($"Unsupported property type {property.Type.Name}"),
      };

      stringBuilder.AppendLine($"{childIndent}  {converterCall}");
    }

    return stringBuilder.ToString();
  }

  private void GenerateClassFooter(StringBuilder stringBuilder, string baseIndent, XmlRpcContextInfo classInfo)
  {
    WriteTypeDeclarationClose(stringBuilder, baseIndent);

    for (int i = classInfo.Parents.Count - 1; i >= 0; i--)
    {
      SyntaxNode syntaxNode = classInfo.Parents[i];
      string indent = new string(' ', i * 2);

      switch (syntaxNode)
      {
        case TypeDeclarationSyntax:
          WriteTypeDeclarationClose(stringBuilder, indent);
          break;
        case BaseNamespaceDeclarationSyntax:
          stringBuilder.AppendLine("}");
          break;
        default:
          throw new Exception($"Unexpected SyntaxNode {classInfo.GetType().FullName}");
      }
    }
  }

  private void WriteTypeDeclarationOpen(StringBuilder stringBuilder, string indent, TypeDeclarationSyntax typeDeclarationSyntax, params string[] implementingTypes)
  {
    if (implementingTypes.Length > 0)
    {
      stringBuilder.AppendLine($"{indent}{typeDeclarationSyntax.Modifiers.ToString()} {typeDeclarationSyntax.Keyword} {typeDeclarationSyntax.Identifier.Text} : {string.Join(", ", implementingTypes)}");
    }
    else
    {
      stringBuilder.AppendLine($"{indent}{typeDeclarationSyntax.Modifiers.ToString()} {typeDeclarationSyntax.Keyword} {typeDeclarationSyntax.Identifier.Text}");
    }

    stringBuilder.AppendLine($"{indent}{{");
  }

  private void WriteTypeDeclarationClose(StringBuilder stringBuilder, string indent)
  {
    stringBuilder.AppendLine($"{indent}}}");
  }
}
