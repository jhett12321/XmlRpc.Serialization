using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace XmlRpc.Serialization.Converters;

public static class XmlRpcConverterFactory
{
  private static readonly XmlRpcArrayConverter<bool> ArrayBoolConverter = new XmlRpcArrayConverter<bool>(XmlRpcValueBooleanConverter.Instance);
  private static readonly XmlRpcArrayConverter<int> ArrayIntConverter = new XmlRpcArrayConverter<int>(XmlRpcValueInt32Converter.Instance);
  private static readonly XmlRpcArrayConverter<double> ArrayDoubleConverter = new XmlRpcArrayConverter<double>(XmlRpcValueDoubleConverter.Instance);
  private static readonly XmlRpcArrayConverter<string> ArrayStringConverter = new XmlRpcArrayConverter<string>(XmlRpcValueStringConverter.Instance);
  private static readonly XmlRpcArrayConverter<byte[]> ArrayBase64Converter = new XmlRpcArrayConverter<byte[]>(XmlRpcValueBase64Converter.Instance);
  private static readonly XmlRpcArrayConverter<DateTime> ArrayDateTimeConverter = new XmlRpcArrayConverter<DateTime>(XmlRpcValueDateTimeConverter.Instance);

  public static XmlRpcValueConverter<T> GetBuiltInValueConverter<T>()
  {
    Type type = typeof(T);

    return Type.GetTypeCode(type) switch
    {
      TypeCode.Boolean => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueBooleanConverter.Instance),
      TypeCode.Int32 => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueInt32Converter.Instance),
      TypeCode.Double => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueDoubleConverter.Instance),
      TypeCode.String => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueStringConverter.Instance),
      TypeCode.DateTime => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueDateTimeConverter.Instance),
      TypeCode.Object when type == typeof(byte[]) => Unsafe.As<XmlRpcValueConverter<T>>(XmlRpcValueBase64Converter.Instance),
      TypeCode.Object when type == typeof(List<bool>) => Unsafe.As<XmlRpcValueConverter<T>>(ArrayBoolConverter),
      TypeCode.Object when type == typeof(List<int>) => Unsafe.As<XmlRpcValueConverter<T>>(ArrayIntConverter),
      TypeCode.Object when type == typeof(List<double>) => Unsafe.As<XmlRpcValueConverter<T>>(ArrayDoubleConverter),
      TypeCode.Object when type == typeof(List<string>) => Unsafe.As<XmlRpcValueConverter<T>>(ArrayStringConverter),
      TypeCode.Object when type == typeof(List<byte[]>) => Unsafe.As<XmlRpcValueConverter<T>>(ArrayBase64Converter),
      TypeCode.Object when type == typeof(List<DateTime>) => Unsafe.As<XmlRpcValueConverter<T>>(ArrayDateTimeConverter),
      _ => throw new SerializationException($"Cannot find XmlRpcValueConverter for type: '{typeof(T).FullName}'"),
    };
  }
}
