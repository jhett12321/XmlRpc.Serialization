using System.IO;
using System.Text;
using System.Xml;

namespace GbxRemote.XmlRpc.Serialization;

internal sealed class SimpleXmlTextWriter : XmlTextWriter
{
  public SimpleXmlTextWriter(Stream w, Encoding? encoding) : base(w, encoding) {}

  public SimpleXmlTextWriter(TextWriter w) : base(w) {}

  public SimpleXmlTextWriter(string filename, Encoding? encoding) : base(filename, encoding) {}

  private bool skipNextAttribute;

  public override void WriteStartAttribute(string? prefix, string localName, string? ns)
  {
    if (prefix == "xmlns" || localName is "type")
    {
      skipNextAttribute = true;
    }
    else
    {
      base.WriteStartAttribute(prefix, localName, ns);
    }
  }

  public override void WriteString(string? text)
  {
    if (!skipNextAttribute)
    {
      base.WriteString(text);
    }
  }

  public override void WriteEndAttribute()
  {
    if (!skipNextAttribute)
    {
      base.WriteEndAttribute();
    }

    skipNextAttribute = false;
  }
}
