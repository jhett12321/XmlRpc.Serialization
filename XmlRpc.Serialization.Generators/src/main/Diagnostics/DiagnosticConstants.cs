using Microsoft.CodeAnalysis;

namespace XmlRpc.Serialization.Generators.Diagnostics;

internal static class DiagnosticConstants
{
  public static readonly DiagnosticDescriptor InvalidConverterType = new DiagnosticDescriptor(
    "XR001",
    "Invalid converter type",
    "Class {0} is not a class type. XML-RPC struct types must be a class.",
    "generation",
    DiagnosticSeverity.Error,
    true);

  public static readonly DiagnosticDescriptor UnknownSyntaxNode = new DiagnosticDescriptor(
    "XR002",
    "Unknown syntax node",
    "This source generator does not support {0} nodes and this class cannot be generated",
    "syntax",
    DiagnosticSeverity.Error,
    true);
}
