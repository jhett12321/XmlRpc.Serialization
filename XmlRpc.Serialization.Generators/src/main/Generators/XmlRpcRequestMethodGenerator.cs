using System;
using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.CodeAnalysis;
using XmlRpc.Serialization.Generators.Extensions;
using XmlRpc.Serialization.Generators.Symbols;

namespace XmlRpc.Serialization.Generators;

internal sealed class XmlRpcRequestMethodGenerator(IndentedTextWriter textWriter, XmlRpcContextInfo contextInfo)
{
  public void GenerateHandleRequestMessage()
  {
    textWriter.BeginScope("public void HandleRequestMessage(string methodName, XmlRpcReader? reader)");
    {
      textWriter.BeginScope("switch (methodName)");
      {
        foreach (XmlRpcMethodInfo method in contextInfo.SerializedType.Methods)
        {
          if (method.Ignored)
          {
            continue;
          }

          GenerateMethodCall(method);
        }

        textWriter.BeginScope("default:");
        {
          textWriter.WriteLine("throw new XmlRpcSerializationException($\"Unknown methodName '{methodName}'.\");");
        }
        textWriter.EndScope();
      }
      textWriter.EndScope();
    }
    textWriter.EndScope();
  }

  private void GenerateMethodCall(XmlRpcMethodInfo method)
  {
    textWriter.BeginScope($"case \"{method.SerializedMethodName}\":");
    {
      if (method.Parameters.Count <= 0)
      {
        textWriter.WriteLine($"{method.MethodName}();");
        textWriter.WriteLine("break;");
        textWriter.EndScope();
        return;
      }

      textWriter.BeginScope("if (reader == null)");
      {
        textWriter.WriteLine($"throw new XmlRpcSerializationException(\"No parameters specified for XML-RPC method '{method.SerializedMethodName}'.\");");
      }
      textWriter.EndScope();
      textWriter.WriteLineNoTabs(string.Empty);
      for (int i = 0; i < method.Parameters.Count; i++)
      {
        XmlRpcMethodParameterInfo parameter = method.Parameters[i];
        textWriter.WriteLine($"{parameter.Type} param{i} = reader.ReadParameter(paramReader => {GetConverterCall(parameter)});");
      }

      string paramsString = string.Join(", ", Enumerable.Range(0, method.Parameters.Count).Select(i => $"param{i}"));
      textWriter.WriteLine($"{method.MethodName}({paramsString});");
      textWriter.WriteLine("break;");
    }
    textWriter.EndScope();
  }

  private string GetConverterCall(XmlRpcMethodParameterInfo parameter)
  {
    return parameter.XmlRpcSerializedType switch
    {
      XmlRpcSerializedType.BuiltIn => $"XmlRpcConverterFactory.GetBuiltInValueConverter<{parameter.Type}>().Deserialize(paramReader)",
      XmlRpcSerializedType.UserArray => $"{contextInfo.Context.Identifier.Text}.{((INamedTypeSymbol)parameter.Type).TypeArguments[0].Name}List.Deserialize(paramReader)",
      XmlRpcSerializedType.UserStruct => $"{contextInfo.Context.Identifier.Text}.{parameter.Type.Name}.Deserialize(paramReader)",
      _ => throw new NotImplementedException($"Unsupported property type {parameter.Type.Name}"),
    };
  }
}
