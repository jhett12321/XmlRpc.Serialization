using Microsoft.CodeAnalysis;
using XmlRpc.Serialization.Generators.Extensions;

namespace XmlRpc.Serialization.Generators.Symbols;

internal sealed record XmlRpcMethodParameterInfo
{
  public ITypeSymbol Type { get; set; }

  public XmlRpcSerializedType XmlRpcSerializedType { get; }

  public XmlRpcMethodParameterInfo(IParameterSymbol parameter)
  {
    Type = parameter.Type;
    XmlRpcSerializedType = Type.GetSerializedType();
  }
}
