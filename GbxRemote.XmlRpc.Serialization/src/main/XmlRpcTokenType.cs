namespace GbxRemote.XmlRpc.Serialization
{
  public enum XmlRpcTokenType
  {
    Unknown = 0,
    None,
    StartXmlDeclaration,
    EndXmlDeclaration,
    StartPayload,
    EndPayload,
    StartParams,
    EndParams,
    StartParam,
    EndParam,
    StartFault,
    EndFault,
    StartValue,
    EndValue,
    StartStruct,
    EndStruct,
    StartMember,
    EndMember,
    StartArray,
    EndArray,
    StartData,
    EndData,
    StartName,
    EndName,
  }
}
