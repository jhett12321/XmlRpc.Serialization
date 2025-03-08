namespace XmlRpc.Serialization.Exceptions;

public sealed class XmlRpcFaultResponse
{
  public int FaultCode { get; set; }
  public string? Message { get; set; }
}
