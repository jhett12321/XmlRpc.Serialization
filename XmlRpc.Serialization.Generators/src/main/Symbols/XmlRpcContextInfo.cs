using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XmlRpc.Serialization.Generators.Symbols;

internal sealed record XmlRpcContextInfo
{
  public XmlRpcContextInfo(ClassDeclarationSyntax classDeclaration, AttributeData attributeData, List<SyntaxNode> parents)
  {
    ClassDeclaration = classDeclaration;
    Parents = parents;
    SerializedType = new XmlRpcTypeInfo((INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!);
  }

  public ClassDeclarationSyntax ClassDeclaration { get; }

  public List<SyntaxNode> Parents { get; }

  // public XmlRpcStructSerializableAttribute(Type type)
  public XmlRpcTypeInfo SerializedType { get; }
}
