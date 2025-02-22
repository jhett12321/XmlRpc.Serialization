using GbxRemote.XmlRpc.Serialization.Attributes;

namespace GbxRemote.XmlRpc.Tests.Models;

[XmlRpcStructSerializable(typeof(ServerStatusResponse))]
public partial class ServerStatusResponseContext;

public sealed class ServerStatusResponse
{
  [XmlRpcPropertyName("Code")]
  public int Code { get; set; }

  [XmlRpcPropertyName("Name")]
  public string Name { get; set; }
}
