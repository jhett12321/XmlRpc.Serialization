using System.Collections.Generic;
using XmlRpc.Serialization.Converters;

namespace XmlRpc.Serialization.Models;

public sealed class XmlRpcRequestMessage
{
  public string MethodName { get; }

  public List<IXmlRpcRequestParameter> Parameters { get; } = [];

  public XmlRpcRequestMessage(string methodName)
  {
    MethodName = methodName;
  }

  public XmlRpcRequestMessage Param<T>(T value, XmlRpcValueConverter<T>? converter = null)
  {
    converter ??= XmlRpcConverterFactory.GetBuiltInValueConverter<T>();
    XmlRpcRequestParameter<T> parameter = new XmlRpcRequestParameter<T>(value, converter);

    Parameters.Add(parameter);
    return this;
  }
}
