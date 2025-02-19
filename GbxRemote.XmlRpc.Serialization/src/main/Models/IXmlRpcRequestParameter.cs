namespace GbxRemote.XmlRpc.Serialization.Models;

public interface IXmlRpcRequestParameter
{
  void Serialize(XmlRpcWriter writer);
}
