﻿using System;
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

  private const string PropertyNameAttributeClassName = "XmlRpcPropertyNameAttribute";
  private const string PropertyIgnoreAttributeClassName = "XmlRpcIgnore";

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    IncrementalValuesProvider<ClassInfo?> classDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName<ClassInfo?>(
        ClassAttributeTypeName,
        (node, _) => node is ClassDeclarationSyntax,
        (ctx, _) => GetSemanticTargetForGeneration(ctx))
      .Where(info => info is not null);

    IncrementalValueProvider<(Compilation, ImmutableArray<ClassInfo?>)> compilation = context.CompilationProvider.Combine(classDeclarations.Collect());
    context.RegisterSourceOutput(compilation, (spc, source) => Execute(source.Item1, source.Item2!, spc));
  }

  private void Execute(Compilation compilation, ImmutableArray<ClassInfo> classes, SourceProductionContext context)
  {
    foreach (ClassInfo classInfo in classes)
    {
      if (classInfo.SerializedType.TypeKind != TypeKind.Class)
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

      context.AddSource(GetFileName(classInfo), GenerateClass(classInfo));
    }
  }

  private ClassInfo? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
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

    return new ClassInfo(classDeclaration, attribute, parentNodes);
  }

  private string GetFileName(ClassInfo classInfo)
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

  private string GenerateClass(ClassInfo classInfo)
  {
    StringBuilder stringBuilder = new StringBuilder();

    GenerateClassHeader(stringBuilder, classInfo);
    GenerateClassBody(stringBuilder, classInfo);
    GenerateClassFooter(stringBuilder, classInfo);

    return stringBuilder.ToString();
  }

  private void GenerateClassHeader(StringBuilder stringBuilder, ClassInfo classInfo)
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
  }

  private void GenerateClassBody(StringBuilder stringBuilder, ClassInfo classInfo)
  {
    string baseIndent = new string(' ', classInfo.Parents.Count * 2);

    WriteTypeDeclarationOpen(stringBuilder, baseIndent, classInfo.ClassDeclaration, $"XmlRpcStructConverter<{classInfo.SerializedType}>");

    string childIndent = new string(' ', (classInfo.Parents.Count + 1) * 2);

    stringBuilder.AppendLine($"{childIndent}public static readonly {classInfo.ClassDeclaration.Identifier.Text} Instance = new {classInfo.ClassDeclaration.Identifier.Text}();");
    stringBuilder.AppendLine();

    List<PropertyInfo> properties = classInfo.SerializedType.GetMembers().OfType<IPropertySymbol>()
      .Select(property => new PropertyInfo(property)).ToList();

    GenerateDeserializer(stringBuilder, childIndent, classInfo, properties);
    stringBuilder.AppendLine();
    GenerateSerializer(stringBuilder, childIndent, classInfo, properties);

    WriteTypeDeclarationClose(stringBuilder, baseIndent);
  }

  private void GenerateDeserializer(StringBuilder stringBuilder, string childIndent, ClassInfo classInfo, List<PropertyInfo> properties)
  {
    stringBuilder.AppendLine($$"""
                               {{childIndent}}protected override void PopulateStructMember({{classInfo.SerializedType}} value, string memberName, XmlRpcReader valueReader)
                               {{childIndent}}{
                               {{childIndent}}  switch (memberName)
                               {{childIndent}}  {
                               {{GeneratePropertyAssignments(childIndent, properties)}}
                               {{childIndent}}  }
                               {{childIndent}}}
                               """);
  }

  private void GenerateSerializer(StringBuilder stringBuilder, string childIndent, ClassInfo classInfo, List<PropertyInfo> properties)
  {
    stringBuilder.AppendLine($$"""
                               {{childIndent}}protected override void WriteStructMembers(XmlRpcWriter writer, {{classInfo.SerializedType}} value)
                               {{childIndent}}{
                               {{GeneratePropertyWriters(childIndent, properties)}}
                               {{childIndent}}}
                               """);
  }

  private string GeneratePropertyAssignments(string childIndent, List<PropertyInfo> properties)
  {
    StringBuilder stringBuilder = new StringBuilder();

    foreach (PropertyInfo property in properties)
    {
      if (property.Ignored)
      {
        continue;
      }

      stringBuilder.AppendLine($"""
                                 {childIndent}    case "{property.SerializedType}":
                                 {childIndent}      value.{property.PropertyName} = XmlRpcConverterFactory.GetBuiltInValueConverter<{property.PropertyType}>().Deserialize(valueReader);
                                 {childIndent}      break;
                                 """);
    }

    return stringBuilder.ToString();
  }

  private string GeneratePropertyWriters(string childIndent, List<PropertyInfo> properties)
  {
    StringBuilder stringBuilder = new StringBuilder();

    foreach (PropertyInfo property in properties)
    {
      if (property.Ignored)
      {
        continue;
      }

      stringBuilder.AppendLine($"{childIndent}  XmlRpcConverterFactory.GetBuiltInValueConverter<{property.PropertyType}>().Serialize(writer, value.{property.PropertyName});");
    }

    return stringBuilder.ToString();
  }

  private void GenerateClassFooter(StringBuilder stringBuilder, ClassInfo classInfo)
  {
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

  private sealed record ClassInfo
  {
    public ClassInfo(ClassDeclarationSyntax classDeclaration, AttributeData attributeData, List<SyntaxNode> parents)
    {
      ClassDeclaration = classDeclaration;
      Parents = parents;
      SerializedType = (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!;
    }

    public ClassDeclarationSyntax ClassDeclaration { get; }

    public List<SyntaxNode> Parents { get; }

    // public XmlRpcStructSerializableAttribute(Type type)
    public INamedTypeSymbol SerializedType { get; }
  }

  private sealed record PropertyInfo
  {
    public string PropertyName { get; }

    public string PropertyType { get; set; }

    public string SerializedType { get; set; }

    public bool Ignored { get; set; }

    public PropertyInfo(IPropertySymbol property)
    {
      PropertyName = property.Name;
      SerializedType = property.Name;
      PropertyType = property.Type.ToString()!;

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
  }
}
