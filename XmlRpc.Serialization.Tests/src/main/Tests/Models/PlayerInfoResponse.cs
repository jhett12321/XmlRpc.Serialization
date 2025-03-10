﻿using XmlRpc.Serialization.Attributes;

namespace XmlRpc.Serialization.Tests.Models;

[XmlRpcStructSerializable(typeof(PlayerInfoResponse))]
public partial class PlayerInfoResponseContext;

public sealed class PlayerInfoResponse
{
  [XmlRpcPropertyName("Uptime")]
  public int Uptime { get; set; }

  [XmlRpcPropertyName("NbrConnection")]
  public int NbrConnection { get; set; }

  [XmlRpcPropertyName("MeanConnectionTime")]
  public int MeanConnectionTime { get; set; }

  [XmlRpcPropertyName("MeanNbrPlayer")]
  public int MeanNbrPlayer { get; set; }

  [XmlRpcPropertyName("RecvNetRate")]
  public int RecvNetRate { get; set; }

  [XmlRpcPropertyName("SendNetRate")]
  public int SendNetRate { get; set; }

  [XmlRpcPropertyName("TotalReceivingSize")]
  public int TotalReceivingSize { get; set; }

  [XmlRpcPropertyName("TotalSendingSize")]
  public int TotalSendingSize { get; set; }

  [XmlRpcPropertyName("PlayerNetInfos")]
  public List<PlayerNetInfo> PlayerNetInfos { get; set; } = null!;
}

public sealed class PlayerNetInfo
{
  [XmlRpcPropertyName("Login")]
  public string Login { get; set; } = null!;

  [XmlRpcPropertyName("IPAddress")]
  public string IpAddress { get; set; } = null!;

  [XmlRpcPropertyName("StateUpdateLatency")]
  public int StateUpdateLatency { get; set; }

  [XmlRpcPropertyName("StateUpdatePeriod")]
  public int StateUpdatePeriod { get; set; }

  [XmlRpcPropertyName("LatestNetworkActivity")]
  public int LatestNetworkActivity { get; set; }

  [XmlRpcPropertyName("PacketLossRate")]
  public double PacketLossRate { get; set; }
}
