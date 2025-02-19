using System;
using System.Collections.Generic;

namespace GbxRemote.XmlRpc.Serialization.Converters;

public class XmlRpcValueArrayConverter<T> : XmlRpcValueConverter<List<T>>
{
  public override List<T> Deserialize(XmlRpcReader reader)
  {
    throw new NotImplementedException();
  }

  public override void Serialize(XmlRpcWriter writer, List<T> value)
  {
    throw new NotImplementedException();
  }
}
