using System.CodeDom.Compiler;

namespace XmlRpc.Serialization.Generators.Extensions;

internal static class TextWriterExtensions
{
  public static void BeginScope(this IndentedTextWriter textWriter, string s)
  {
    textWriter.WriteLine(s);
    textWriter.WriteLine("{");
    textWriter.Indent++;
  }

  public static void BeginScope(this IndentedTextWriter textWriter)
  {
    textWriter.WriteLine("{");
    textWriter.Indent++;
  }

  public static void EndScope(this IndentedTextWriter textWriter)
  {
    textWriter.Indent--;
    textWriter.WriteLine("}");
  }
}
