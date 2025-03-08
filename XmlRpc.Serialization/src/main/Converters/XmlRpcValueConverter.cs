namespace XmlRpc.Serialization.Converters;

public abstract class XmlRpcValueConverter<T>
{
  public abstract T Deserialize(XmlRpcReader reader);

  public abstract void Serialize(XmlRpcWriter writer, T value);
}
