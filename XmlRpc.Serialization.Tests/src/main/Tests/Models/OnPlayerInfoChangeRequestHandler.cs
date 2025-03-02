using XmlRpc.Serialization.Attributes;

namespace XmlRpc.Serialization.Tests.Models;

[XmlRpcRequestHandler]
public partial class OnPlayerInfoChangeRequestHandler(Action<OnPlayerInfoChangeRequest> handler) : IXmlRpcRequestHandler
{
  [XmlRpcMethodName("TrackMania.PlayerInfoChanged")]
  public void OnPlayerInfoChange(OnPlayerInfoChangeRequest request)
  {
    handler(request);
  }
}
