using GbxRemote.XmlRpc.Serialization.Exceptions;
using GbxRemote.XmlRpc.Serialization.Models;

namespace GbxRemote.XmlRpc.Serialization.Converters
{
  internal sealed class XmlRpcFaultResponseConverter : XmlRpcStructConverter<XmlRpcFaultResponse>
  {
    public static readonly XmlRpcFaultResponseConverter Instance = new XmlRpcFaultResponseConverter();

    protected override void PopulateStructMember(XmlRpcFaultResponse value, string memberName, XmlRpcReader valueReader)
    {
      switch (memberName)
      {
        case "faultCode":
          value.FaultCode = valueReader.GetInt32();
          break;
        case "faultString":
          value.Message = valueReader.GetString();
          break;
        default:
          throw new XmlRpcSerializationException("Unknown struct member name: " + memberName);
      }
    }

    protected override void WriteStructMembers(XmlRpcWriter writer, XmlRpcFaultResponse value)
    {
      writer.Write(XmlRpcTokenType.StartMember);
      writer.WriteElement("name", "faultCode");
      XmlRpcConverterFactory.GetBuiltInValueConverter<int>().Serialize(writer, value.FaultCode);
      writer.Write(XmlRpcTokenType.EndMember);

      writer.Write(XmlRpcTokenType.StartMember);
      writer.WriteElement("name", "faultString");
      XmlRpcConverterFactory.GetBuiltInValueConverter<string>().Serialize(writer, value.Message ?? string.Empty);
      writer.Write(XmlRpcTokenType.EndMember);
    }
  }
}
