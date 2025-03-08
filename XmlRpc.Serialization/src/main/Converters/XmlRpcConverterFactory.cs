using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using XmlRpc.Serialization.Converters.Extended;

namespace XmlRpc.Serialization.Converters;

public static class XmlRpcConverterFactory
{
  private static readonly Dictionary<Type, object> BuiltInConverters = new Dictionary<Type, object>
  {
    [typeof(bool)] = XmlRpcValueBooleanConverter.Converter,
    [typeof(int)] = XmlRpcValueInt32Converter.Converter,
    [typeof(double)] = XmlRpcValueDoubleConverter.Converter,
    [typeof(string)] = XmlRpcValueStringConverter.Converter,
    [typeof(DateTime)] = XmlRpcValueDateTimeConverter.Converter,
    [typeof(byte[])] = XmlRpcValueBase64Converter.Converter,

    [typeof(List<bool>)] = XmlRpcValueBooleanConverter.ArrayConverter,
    [typeof(List<int>)] = XmlRpcValueInt32Converter.ArrayConverter,
    [typeof(List<double>)] = XmlRpcValueDoubleConverter.ArrayConverter,
    [typeof(List<string>)] = XmlRpcValueStringConverter.ArrayConverter,
    [typeof(List<DateTime>)] = XmlRpcValueDateTimeConverter.ArrayConverter,
    [typeof(List<byte[]>)] = XmlRpcValueBase64Converter.ArrayConverter,

    [typeof(byte)] = XmlRpcValueByteConverter.Converter,
    [typeof(char)] = XmlRpcValueCharConverter.Converter,
    [typeof(short)] = XmlRpcValueInt16Converter.Converter,
    [typeof(sbyte)] = XmlRpcValueSByteConverter.Converter,
    [typeof(float)] = XmlRpcValueSingleConverter.Converter,
    [typeof(ushort)] = XmlRpcValueUInt16Converter.Converter,

    [typeof(List<byte>)] = XmlRpcValueByteConverter.ArrayConverter,
    [typeof(List<char>)] = XmlRpcValueCharConverter.ArrayConverter,
    [typeof(List<short>)] = XmlRpcValueInt16Converter.ArrayConverter,
    [typeof(List<sbyte>)] = XmlRpcValueSByteConverter.ArrayConverter,
    [typeof(List<float>)] = XmlRpcValueSingleConverter.ArrayConverter,
    [typeof(List<ushort>)] = XmlRpcValueUInt16Converter.ArrayConverter,
  };

  public static XmlRpcValueConverter<T> GetBuiltInValueConverter<T>()
  {
    if (BuiltInConverters.TryGetValue(typeof(T), out object? converter))
    {
      return (XmlRpcValueConverter<T>)converter;
    }

    throw new SerializationException($"Cannot find XmlRpcValueConverter for type: '{typeof(T).FullName}'");
  }
}
