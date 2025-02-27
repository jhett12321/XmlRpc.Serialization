using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using XmlRpc.Serialization.Exceptions;

namespace XmlRpc.Serialization;

public sealed class XmlRpcReader : IDisposable
{
  private static readonly XmlReaderSettings ReaderSettings = new XmlReaderSettings
  {
    IgnoreWhitespace = true,
    IgnoreComments = true,
    IgnoreProcessingInstructions = true,
    ConformanceLevel = ConformanceLevel.Fragment,
    DtdProcessing = DtdProcessing.Ignore,
    ValidationType = ValidationType.None,
  };

  private readonly XmlReader xmlReader;

  private XmlRpcTokenType tokenType;
  private bool tokenTypeDirty;

  public XmlRpcTokenType TokenType
  {
    get
    {
      if (tokenTypeDirty)
      {
        UpdateCurrentNode();
      }

      return tokenType;
    }
  }

  public string Name => xmlReader.Name;

  public string Value => xmlReader.Value;

  public XmlRpcReader(Stream stream)
  {
    xmlReader = XmlReader.Create(stream, ReaderSettings);
  }

  public XmlRpcReader(TextReader textReader)
  {
    xmlReader = XmlReader.Create(textReader, ReaderSettings);
  }

  public XmlRpcReader(XmlReader xmlReader)
  {
    this.xmlReader = xmlReader;
  }

  public void Read(XmlRpcTokenType expectedTokenType)
  {
    bool hasNextToken = Read();
    if (!hasNextToken && expectedTokenType != XmlRpcTokenType.None)
    {
      throw new XmlRpcSerializationException("Unexpected end of stream.");
    }

    if (expectedTokenType != TokenType)
    {
      throw new XmlRpcSerializationException($"Expected node type '{expectedTokenType}' in stream, but got '{TokenType}'. XmlReader node: '{xmlReader.NodeType}', name: '{xmlReader.Name}'");
    }
  }

  public bool Read()
  {
    bool hasValue = xmlReader.Read();
    tokenTypeDirty = true;
    return hasValue;
  }

  public string ReadElementContentAsString()
  {
    string retVal = xmlReader.ReadElementContentAsString();
    tokenTypeDirty = true;
    return retVal;
  }

  public XmlRpcTokenType ReadNextToken()
  {
    Read();
    return TokenType;
  }

  public XmlRpcReader ReadSubtree(bool copy)
  {
    XmlRpcReader retVal;
    if (copy)
    {
      string xml = xmlReader.ReadOuterXml();
      retVal = new XmlRpcReader(new StringReader(xml));
    }
    else
    {
      retVal = new XmlRpcReader(xmlReader.ReadSubtree());
    }

    tokenTypeDirty = true;
    return retVal;
  }

  public void ReadStructMember(Action<string, XmlRpcReader> readValue)
  {
    string? name = null;
    XmlRpcReader? valueReader = null;

    if (TokenType != XmlRpcTokenType.StartMember)
    {
      throw new XmlRpcSerializationException($"Expected node type '{XmlRpcTokenType.StartMember}'");
    }

    Read();
    switch (TokenType)
    {
      case XmlRpcTokenType.StartName:
        name = ReadElementContentAsString();
        break;
      case XmlRpcTokenType.StartValue:
        valueReader = ReadSubtree(true);
        break;
    }

    switch (TokenType)
    {
      case XmlRpcTokenType.StartName:
        name = ReadElementContentAsString();
        break;
      case XmlRpcTokenType.StartValue:
        valueReader = ReadSubtree(false);
        break;
    }

    if (name == null || valueReader == null)
    {
      throw new XmlRpcSerializationException($"Struct member is incomplete: '{name}'");
    }

    readValue(name, valueReader);
    valueReader.Dispose();

    if (TokenType != XmlRpcTokenType.EndMember)
    {
      Read(XmlRpcTokenType.EndMember);
    }

    if (TokenType != XmlRpcTokenType.EndMember)
    {
      throw new XmlRpcSerializationException($"Expected node type '{XmlRpcTokenType.EndMember}'");
    }
  }

  public int GetInt32()
  {
    return int.Parse(ReadSimpleValueNode("int", "i4"));
  }

  public bool GetBoolean()
  {
    return ReadSimpleValueNode("boolean") != "0";
  }

  public string GetString()
  {
    if (TokenType != XmlRpcTokenType.StartValue)
    {
      Read(XmlRpcTokenType.StartValue);
    }

    Read();

    string value;
    if (xmlReader.NodeType == XmlNodeType.Text)
    {
      value = xmlReader.ReadContentAsString();
    }
    else
    {
      ValidateValueNode(XmlNodeType.Element, "string");
      value = ReadElementContentAsString();
    }

    tokenTypeDirty = true;
    ValidateValueNode(XmlNodeType.EndElement, "value");

    return value;
  }

  public double GetDouble()
  {
    return double.Parse(ReadSimpleValueNode("double"));
  }

  public DateTime GetDateTime()
  {
    return DateTime.ParseExact(ReadSimpleValueNode("dateTime.iso8601"), "yyyyMMddTHH:mm:ss", CultureInfo.InvariantCulture);
  }

  public byte[] GetBase64Bytes()
  {
    string base64String = ReadSimpleValueNode("base64");
    return Convert.FromBase64String(base64String);
  }

  public T GetEnum<T>() where T : Enum
  {
    if (Unsafe.SizeOf<T>() != Unsafe.SizeOf<int>())
    {
      throw new ArgumentOutOfRangeException(nameof(T), "Specified enum must be backed by a signed int32 (int)");
    }

    int value = GetInt32();
    return Unsafe.As<int, T>(ref value);
  }

  private void UpdateCurrentNode()
  {
    tokenType = xmlReader.NodeType switch
    {
      XmlNodeType.None => XmlRpcTokenType.None,
      XmlNodeType.XmlDeclaration => XmlRpcTokenType.StartXmlDeclaration,
      XmlNodeType.Element when xmlReader.Name == "methodResponse" => XmlRpcTokenType.StartMethodResponse,
      XmlNodeType.Element when xmlReader.Name == "methodCall" => XmlRpcTokenType.StartMethodCall,
      XmlNodeType.Element when xmlReader.Name == "params" => XmlRpcTokenType.StartParams,
      XmlNodeType.Element when xmlReader.Name == "param" => XmlRpcTokenType.StartParam,
      XmlNodeType.Element when xmlReader.Name == "fault" => XmlRpcTokenType.StartFault,
      XmlNodeType.Element when xmlReader.Name == "value" => XmlRpcTokenType.StartValue,
      XmlNodeType.Element when xmlReader.Name == "struct" => XmlRpcTokenType.StartStruct,
      XmlNodeType.Element when xmlReader.Name == "member" => XmlRpcTokenType.StartMember,
      XmlNodeType.Element when xmlReader.Name == "name" => XmlRpcTokenType.StartName,
      XmlNodeType.Element when xmlReader.Name == "array" => XmlRpcTokenType.StartArray,
      XmlNodeType.Element when xmlReader.Name == "data" => XmlRpcTokenType.StartData,
      XmlNodeType.EndElement when xmlReader.Name == "methodResponse" => XmlRpcTokenType.EndMethodResponse,
      XmlNodeType.EndElement when xmlReader.Name == "methodCall" => XmlRpcTokenType.EndMethodCall,
      XmlNodeType.EndElement when xmlReader.Name == "params" => XmlRpcTokenType.EndParams,
      XmlNodeType.EndElement when xmlReader.Name == "params" => XmlRpcTokenType.EndParams,
      XmlNodeType.EndElement when xmlReader.Name == "param" => XmlRpcTokenType.EndParam,
      XmlNodeType.EndElement when xmlReader.Name == "fault" => XmlRpcTokenType.EndFault,
      XmlNodeType.EndElement when xmlReader.Name == "value" => XmlRpcTokenType.EndValue,
      XmlNodeType.EndElement when xmlReader.Name == "struct" => XmlRpcTokenType.EndStruct,
      XmlNodeType.EndElement when xmlReader.Name == "member" => XmlRpcTokenType.EndMember,
      XmlNodeType.EndElement when xmlReader.Name == "name" => XmlRpcTokenType.EndName,
      XmlNodeType.EndElement when xmlReader.Name == "array" => XmlRpcTokenType.EndArray,
      XmlNodeType.EndElement when xmlReader.Name == "data" => XmlRpcTokenType.EndData,
      _ => XmlRpcTokenType.Unknown,
    };

    tokenTypeDirty = false;
  }

  private string ReadSimpleValueNode(params string[] expectedNodeNames)
  {
    if (TokenType != XmlRpcTokenType.StartValue)
    {
      Read(XmlRpcTokenType.StartValue);
    }

    Read();
    ValidateValueNode(XmlNodeType.Element, expectedNodeNames);

    string value = ReadElementContentAsString();
    tokenTypeDirty = true;

    ValidateValueNode(XmlNodeType.EndElement, "value");

    return value;
  }

  private void ValidateValueNode(XmlNodeType expectedNodeType, params string[] expectedNodeNames)
  {
    if (xmlReader.NodeType != expectedNodeType)
    {
      throw new XmlRpcSerializationException($"Expected node type '{expectedNodeType}' in stream, but got '{TokenType}'. XmlReader node: '{xmlReader.NodeType}', name: '{xmlReader.Name}'");
    }

    if (expectedNodeNames.Length > 0 && Array.IndexOf(expectedNodeNames, xmlReader.Name) == -1)
    {
      throw new XmlRpcSerializationException($"Expected node with name/s '{string.Join(',', expectedNodeNames)}' in stream, but got '{xmlReader.Name}'");
    }
  }

  public void Dispose()
  {
    xmlReader.Dispose();
  }
}
