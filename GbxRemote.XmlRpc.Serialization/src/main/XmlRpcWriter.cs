using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace GbxRemote.XmlRpc.Serialization
{
  public sealed class XmlRpcWriter : IDisposable
  {
    private static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings
    {
      Encoding = new XmlRpcUtf8Encoding(),
      ConformanceLevel = ConformanceLevel.Auto,
    };

    private readonly XmlWriter xmlWriter;

    public XmlRpcWriter(Stream stream)
    {
      xmlWriter = XmlWriter.Create(stream, WriterSettings);
    }

    public XmlRpcWriter(TextWriter textWriter)
    {
      xmlWriter = XmlWriter.Create(textWriter, WriterSettings);
    }

    public void Write(XmlRpcTokenType tokenType)
    {
      switch (tokenType)
      {
        case XmlRpcTokenType.StartXmlDeclaration:
          xmlWriter.WriteStartDocument();
          break;
        case XmlRpcTokenType.EndXmlDeclaration:
          xmlWriter.WriteEndDocument();
          break;
        case XmlRpcTokenType.StartMethodCall:
          xmlWriter.WriteStartElement("methodCall");
          break;
        case XmlRpcTokenType.EndMethodCall:
          xmlWriter.WriteEndElement();
          break;
        case XmlRpcTokenType.StartMethodResponse:
          xmlWriter.WriteStartElement("methodResponse");
          break;
        case XmlRpcTokenType.EndMethodResponse:
          xmlWriter.WriteEndElement();
          break;
        case XmlRpcTokenType.StartParams:
          xmlWriter.WriteStartElement("params");
          break;
        case XmlRpcTokenType.EndParams:
          xmlWriter.WriteEndElement();
          break;
        case XmlRpcTokenType.StartParam:
          xmlWriter.WriteStartElement("param");
          break;
        case XmlRpcTokenType.EndParam:
          xmlWriter.WriteEndElement();
          break;
        case XmlRpcTokenType.StartFault:
          xmlWriter.WriteStartElement("fault");
          break;
        case XmlRpcTokenType.EndFault:
          xmlWriter.WriteEndElement();
          break;
        case XmlRpcTokenType.StartValue:
          xmlWriter.WriteStartElement("value");
          break;
        case XmlRpcTokenType.EndValue:
          xmlWriter.WriteEndElement();
          break;
        case XmlRpcTokenType.StartStruct:
          xmlWriter.WriteStartElement("struct");
          break;
        case XmlRpcTokenType.EndStruct:
          xmlWriter.WriteEndElement();
          break;
        case XmlRpcTokenType.StartMember:
          xmlWriter.WriteStartElement("member");
          break;
        case XmlRpcTokenType.EndMember:
          xmlWriter.WriteEndElement();
          break;
        case XmlRpcTokenType.StartArray:
          xmlWriter.WriteStartElement("array");
          break;
        case XmlRpcTokenType.EndArray:
          xmlWriter.WriteEndElement();
          break;
        case XmlRpcTokenType.StartData:
          xmlWriter.WriteStartElement("data");
          break;
        case XmlRpcTokenType.EndData:
          xmlWriter.WriteEndElement();
          break;
        case XmlRpcTokenType.StartName:
          xmlWriter.WriteStartElement("name");
          break;
        case XmlRpcTokenType.EndName:
          xmlWriter.WriteEndElement();
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
      }
    }

    public void WriteElement(string elementName, string value)
    {
      xmlWriter.WriteElementString(elementName, value);
    }

    public void WriteInt32Value(int value)
    {
      WriteSimpleValueNode("i4", value.ToString());
    }

    public void WriteBooleanValue(bool value)
    {
      WriteSimpleValueNode("boolean", value ? "1" : "0");
    }

    public void WriteStringValue(string value)
    {
      WriteSimpleValueNode("string", value);
    }

    public void WriteDoubleValue(double value)
    {
      WriteSimpleValueNode("double", value.ToString("0." + new string('#', 339)).TrimEnd('0'));
    }

    public void WriteDateTimeValue(DateTime value)
    {
      WriteSimpleValueNode("dateTime.iso8601", value.ToString("yyyyMMddTHH:mm:ss", CultureInfo.InvariantCulture));
    }

    public void WriteBase64BytesValue(byte[] value)
    {
      WriteSimpleValueNode("base64", Convert.ToBase64String(value));
    }

    public void WriteEnumValue<T>(T value) where T : Enum
    {
      if (Unsafe.SizeOf<T>() != Unsafe.SizeOf<int>())
      {
        throw new ArgumentOutOfRangeException(nameof(T), "Specified enum must be backed by a signed int32 (int)");
      }

      WriteSimpleValueNode("int", Unsafe.As<T, int>(ref value).ToString());
    }

    private void WriteSimpleValueNode(string nodeName, string value)
    {
      Write(XmlRpcTokenType.StartValue);
      xmlWriter.WriteStartElement(nodeName);

      xmlWriter.WriteString(value);

      xmlWriter.WriteEndElement();
      Write(XmlRpcTokenType.EndValue);
    }

    public void Dispose()
    {
      xmlWriter.Dispose();
    }
  }
}
