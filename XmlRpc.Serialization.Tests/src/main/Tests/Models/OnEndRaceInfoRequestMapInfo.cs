using XmlRpc.Serialization.Attributes;

namespace XmlRpc.Serialization.Tests.Models;

[XmlRpcStructSerializable(typeof(OnEndRaceInfoRequestMapInfo))]
public partial class OnEndRaceInfoRequestMapInfoContext;

public sealed class OnEndRaceInfoRequestMapInfo
{
  [XmlRpcPropertyName("UId")]
  public string UId { get; set; } = null!;

  [XmlRpcPropertyName("Name")]
  public string Name { get; set; } = null!;

  [XmlRpcPropertyName("FileName")]
  public string FileName { get; set; } = null!;

  [XmlRpcPropertyName("Author")]
  public string Author { get; set; } = null!;

  [XmlRpcPropertyName("AuthorNickname")]
  public string AuthorNickname { get; set; } = null!;

  [XmlRpcPropertyName("Environnement")]
  public string Environment { get; set; } = null!;

  [XmlRpcPropertyName("Mood")]
  public string Mood { get; set; } = null!;

  [XmlRpcPropertyName("BronzeTime")]
  public int BronzeTime { get; set; }

  [XmlRpcPropertyName("SilverTime")]
  public int SilverTime { get; set; }

  [XmlRpcPropertyName("GoldTime")]
  public int GoldTime { get; set; }

  [XmlRpcPropertyName("AuthorTime")]
  public int AuthorTime { get; set; }

  [XmlRpcPropertyName("CopperPrice")]
  public int CopperPrice { get; set; }

  [XmlRpcPropertyName("LapRace")]
  public bool LapRace { get; set; }

  [XmlRpcPropertyName("NbLaps")]
  public int NbLaps { get; set; }

  [XmlRpcPropertyName("NbCheckpoints")]
  public int NbCheckpoints { get; set; }

  [XmlRpcPropertyName("MapType")]
  public string MapType { get; set; } = null!;

  [XmlRpcPropertyName("MapStyle")]
  public string MapStyle { get; set; } = null!;
}
