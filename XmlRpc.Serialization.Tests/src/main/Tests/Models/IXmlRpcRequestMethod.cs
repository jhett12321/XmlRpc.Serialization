namespace XmlRpc.Serialization.Tests.Models;

public interface IXmlRpcRequestMethod
{
  void HandleRequestMessage(XmlRpcReader? reader);
}

public interface IXmlRpcRequestMethod<out T> : IXmlRpcRequestMethod where T : Delegate
{
  T Handler { get; }
}
