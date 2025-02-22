using GbxRemote.XmlRpc.Serialization.Attributes;

namespace GbxRemote.XmlRpc.Tests.Models;

[XmlRpcStructSerializable(typeof(PlayerInfoChangedRequest))]
internal partial class PlayerInfoChangedRequestContext;

public sealed class PlayerInfoChangedRequest
{
  [XmlRpcPropertyName("Login")]
  public string Login { get; set; }

  [XmlRpcPropertyName("NickName")]
  public string NickName { get; set; }

  [XmlRpcPropertyName("PlayerId")]
  public int PlayerId { get; set; }

  [XmlRpcPropertyName("TeamId")]
  public int TeamId { get; set; }

  [XmlRpcPropertyName("SpectatorStatus")]
  public int SpectatorStatus { get; set; }

  [XmlRpcPropertyName("LadderRanking")]
  public int LadderRanking { get; set; }

  [XmlRpcPropertyName("Flags")]
  public int Flags { get; set; }

  [XmlRpcPropertyName("LadderScore")]
  public double LadderScore { get; set; }
}
