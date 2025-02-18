using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace GbxRemote.XmlRpc.Serialization.Models;

public class XmlRpcResponseEnum<T> : IXmlRpcResponseValue<T> where T : Enum
{
  public T? Value { get; private set; }

  public XmlRpcResponseEnum()
  {
    if (Unsafe.SizeOf<T>() != Unsafe.SizeOf<int>())
    {
      throw new ArgumentOutOfRangeException(nameof(T), "Specified enum must be backed by a signed int32 (int)");
    }
  }

  public bool IsValidType(string? typeId)
  {
    return typeId is "i4" or "int";
  }

  public void DeserializeXml(XElement element)
  {
    int enumValue = int.Parse(element.Value);
    Value = Unsafe.As<int, T>(ref enumValue);
  }

  public static implicit operator T?(XmlRpcResponseEnum<T> xmlRpcResponseEnum)
  {
    return xmlRpcResponseEnum.Value;
  }
}
