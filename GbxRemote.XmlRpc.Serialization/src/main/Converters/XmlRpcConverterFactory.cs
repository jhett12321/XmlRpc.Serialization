using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace GbxRemote.XmlRpc.Serialization.Converters;

public static class XmlRpcConverterFactory
{
  public static XmlRpcValueConverter<T> GetBuiltInValueConverter<T>()
  {
    return Type.GetTypeCode(typeof(T)) switch
    {
      TypeCode.Boolean => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueBooleanConverter.Instance),
      TypeCode.Int32 => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueInt32Converter.Instance),
      TypeCode.Double => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueDoubleConverter.Instance),
      TypeCode.String => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueStringConverter.Instance),
      TypeCode.Object when typeof(T) == typeof(byte[]) => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueBase64Converter.Instance),
      TypeCode.DateTime => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueDateTimeConverter.Instance),
      _ => throw new SerializationException($"Cannot find XmlRpcValueConverter for type: '{typeof(T).FullName}'"),
    };
  }
}
