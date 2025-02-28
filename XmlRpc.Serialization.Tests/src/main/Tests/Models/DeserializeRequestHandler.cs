namespace XmlRpc.Serialization.Tests.Models;

public sealed class DeserializeRequestHandler : IXmlRpcRequestHandler
{
  private readonly Dictionary<string, IXmlRpcRequestMethod> methods = new Dictionary<string, IXmlRpcRequestMethod>();

  public void AddRequestMethod(string methodName, IXmlRpcRequestMethod method)
  {
    methods.Add(methodName, method);
  }

  public void HandleRequestMessage(string methodName, XmlRpcReader? reader)
  {
    if (methods.TryGetValue(methodName, out IXmlRpcRequestMethod? method))
    {
      method.HandleRequestMessage(reader);
    }
  }
}
