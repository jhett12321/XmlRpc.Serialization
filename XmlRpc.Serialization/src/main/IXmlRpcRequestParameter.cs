namespace XmlRpc.Serialization;

public interface IXmlRpcRequestParameter
{
  void Serialize(XmlRpcWriter writer);
}
