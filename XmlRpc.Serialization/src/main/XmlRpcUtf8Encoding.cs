using System.Text;

namespace XmlRpc.Serialization;

internal sealed class XmlRpcUtf8Encoding() : UTF8Encoding(false)
{
  public override string HeaderName => base.HeaderName.ToUpper();
  public override string WebName => base.WebName.ToUpper();
  public override string BodyName => base.BodyName.ToUpper();
}
