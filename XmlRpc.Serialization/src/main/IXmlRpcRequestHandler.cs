namespace XmlRpc.Serialization;

public interface IXmlRpcRequestHandler
{
  void HandleRequestMessage(string methodName, XmlRpcReader? reader);
}
