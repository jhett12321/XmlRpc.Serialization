using GbxRemote.XmlRpc.Serialization.Converters;

namespace GbxRemote.XmlRpc.Serialization.Models;

public sealed class XmlRpcRequestParameter<T> : IXmlRpcRequestParameter
{
  private readonly XmlRpcValueConverter<T> converter;

  public T Value { get; }

  public XmlRpcRequestParameter(T value, XmlRpcValueConverter<T>? converter = null)
  {
    Value = value;

    converter ??= XmlRpcConverterFactory.GetBuiltInValueConverter<T>();
    this.converter = converter;
  }

  public void Serialize(XmlRpcWriter writer)
  {
    converter.Serialize(writer, Value);
  }

  public static implicit operator XmlRpcRequestParameter<T>(T parameter)
  {
    return new XmlRpcRequestParameter<T>(parameter);
  }
}
