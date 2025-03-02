using XmlRpc.Serialization.Attributes;

namespace XmlRpc.Serialization.Tests.Models;

[XmlRpcStructSerializable(typeof(OnEndRaceInfoRequestPlayerInfo))]
public partial class OnEndRaceInfoRequestPlayerInfoContext;

public sealed class OnEndRaceInfoRequestPlayerInfo
{
  [XmlRpcPropertyName("Login")]
  public string Login { get; set; } = null!;

  [XmlRpcPropertyName("NickName")]
  public string NickName { get; set; } = null!;

  [XmlRpcPropertyName("PlayerId")]
  public int PlayerId { get; set; }

  [XmlRpcPropertyName("Rank")]
  public int Rank { get; set; }
}
