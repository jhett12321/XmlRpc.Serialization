using XmlRpc.Serialization.Attributes;

namespace XmlRpc.Serialization.Tests.Models;

[XmlRpcRequestHandler]
public sealed partial class OnEndRaceRequestHandler(Action<List<OnEndRaceInfoRequestPlayerInfo>, OnEndRaceInfoRequestMapInfo> handler) : IXmlRpcRequestHandler
{
  [XmlRpcMethodName("TrackMania.EndRace")]
  public void OnEndRaceRequest(List<OnEndRaceInfoRequestPlayerInfo> playerInfos, OnEndRaceInfoRequestMapInfo mapInfo)
  {
    handler(playerInfos, mapInfo);
  }
}
