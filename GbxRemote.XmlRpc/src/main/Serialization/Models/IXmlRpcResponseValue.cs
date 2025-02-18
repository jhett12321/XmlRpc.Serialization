using System.Xml.Linq;

namespace GbxRemote.XmlRpc.Serialization.Models;

public interface IXmlRpcResponseValue
{
  object? Value { get; }

  public bool IsValidType(string? typeId);

  public void DeserializeXml(XElement element);
}

public interface IXmlRpcResponseValue<out T> : IXmlRpcResponseValue
{
  object? IXmlRpcResponseValue.Value => Value;

  new T? Value { get; }
}
