using XmlRpc.Serialization.Attributes;

namespace XmlRpc.Serialization.Tests.Models;

[XmlRpcStructSerializable(typeof(ServerStatusResponse))]
public partial class ServerStatusResponseContext;

public sealed class ServerStatusResponse
{
  [XmlRpcPropertyName("Code")]
  public int Code { get; set; }

  [XmlRpcPropertyName("Name")]
  public string Name { get; set; } = null!;
}
