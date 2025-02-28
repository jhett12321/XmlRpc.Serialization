using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using XmlRpc.Serialization.Exceptions;

namespace XmlRpc.Serialization;

/// <summary>
/// Represents a <see cref="XmlReader"/> configured to parse valid XML-RPC tokens, providing forward-only access to XML data.
/// </summary>
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

  /// <summary>
  /// Gets the type of the last processed XML-RPC token.
  /// </summary>
  public XmlRpcTokenType TokenType => GetCurrentToken();

  /// <summary>
  /// Gets the qualified name of the current node.
  /// </summary>
  public string Name => xmlReader.Name;

  /// <summary>
  /// Gets the text value of the current node.
  /// </summary>
  public string Value => xmlReader.Value;

  /// <summary>
  /// Creates a new <see cref="XmlRpcReader"/> using the specified stream.
  /// </summary>
  /// <param name="stream">The stream that contains the XML data.</param>
  public XmlRpcReader(Stream stream)
  {
    xmlReader = XmlReader.Create(stream, ReaderSettings);
  }

  /// <summary>
  /// Creates a new <see cref="XmlRpcReader"/> using the specified <see cref="TextReader"/>.
  /// </summary>
  /// <param name="textReader">The text reader from which to read the XML data.</param>
  public XmlRpcReader(TextReader textReader)
  {
    xmlReader = XmlReader.Create(textReader, ReaderSettings);
  }

  /// <summary>
  /// Creates a new <see cref="XmlRpcReader"/> using the specified <see cref="XmlReader"/>.
  /// </summary>
  /// <param name="xmlReader">The XML reader from which to read the XML data.</param>
  public XmlRpcReader(XmlReader xmlReader)
  {
    this.xmlReader = xmlReader;
  }

  /// <summary>
  /// Reads the next XML-RPC token from the input source, and checks if the token matches the specified <see cref="XmlRpcTokenType"/>.
  /// </summary>
  /// <param name="expectedTokenType">The expected XML-RPC token type.</param>
  /// <exception cref="XmlRpcSerializationException">Thrown if the next XML-RPC token does not match the value specified in the <see cref="expectedTokenType"/> parameter.</exception>
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

  public void ReadOrAdvance(XmlRpcTokenType expectedTokenType)
  {
    if (TokenType == expectedTokenType)
    {
      return;
    }

    Read(expectedTokenType);
  }

  /// <summary>
  /// Reads the next XML-RPC token from the input source.
  /// </summary>
  /// <returns>True if the token was read successfully, else false.</returns>
  public bool Read()
  {
    bool hasValue = xmlReader.Read();
    return hasValue;
  }

  /// <summary>
  /// Reads the current XML element and returns the contents as a <see cref="string"/>.
  /// </summary>
  /// <returns></returns>
  public string ReadElementContentAsString()
  {
    string retVal = xmlReader.ReadElementContentAsString();
    return retVal;
  }

  /// <summary>
  /// Reads the next XML-RPC token from the input source.
  /// </summary>
  /// <returns>The XML-RPC token type after advancing to the next token.</returns>
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

    ReadOrAdvance(XmlRpcTokenType.EndMember);
  }

  public T ReadParameter<T>(Func<XmlRpcReader, T> readValue)
  {
    ReadOrAdvance(XmlRpcTokenType.StartParam);
    T retVal = readValue(this);
    ReadOrAdvance(XmlRpcTokenType.EndParam);

    return retVal;
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
    ReadOrAdvance(XmlRpcTokenType.StartValue);

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

  private XmlRpcTokenType GetCurrentToken()
  {
    XmlNodeType nodeType = xmlReader.NodeType;
    string? nodeName = nodeType is XmlNodeType.Element or XmlNodeType.EndElement ? xmlReader.Name : null;

    return nodeType switch
    {
      XmlNodeType.None => XmlRpcTokenType.None,
      XmlNodeType.XmlDeclaration => XmlRpcTokenType.StartXmlDeclaration,
      XmlNodeType.Element when nodeName == "methodResponse" => XmlRpcTokenType.StartMethodResponse,
      XmlNodeType.Element when nodeName == "methodCall" => XmlRpcTokenType.StartMethodCall,
      XmlNodeType.Element when nodeName == "methodName" => XmlRpcTokenType.StartMethodName,
      XmlNodeType.Element when nodeName == "params" => XmlRpcTokenType.StartParams,
      XmlNodeType.Element when nodeName == "param" => XmlRpcTokenType.StartParam,
      XmlNodeType.Element when nodeName == "fault" => XmlRpcTokenType.StartFault,
      XmlNodeType.Element when nodeName == "value" => XmlRpcTokenType.StartValue,
      XmlNodeType.Element when nodeName == "struct" => XmlRpcTokenType.StartStruct,
      XmlNodeType.Element when nodeName == "member" => XmlRpcTokenType.StartMember,
      XmlNodeType.Element when nodeName == "name" => XmlRpcTokenType.StartName,
      XmlNodeType.Element when nodeName == "array" => XmlRpcTokenType.StartArray,
      XmlNodeType.Element when nodeName == "data" => XmlRpcTokenType.StartData,
      XmlNodeType.EndElement when nodeName == "methodResponse" => XmlRpcTokenType.EndMethodResponse,
      XmlNodeType.EndElement when nodeName == "methodCall" => XmlRpcTokenType.EndMethodCall,
      XmlNodeType.EndElement when nodeName == "methodName" => XmlRpcTokenType.EndMethodName,
      XmlNodeType.EndElement when nodeName == "params" => XmlRpcTokenType.EndParams,
      XmlNodeType.EndElement when nodeName == "params" => XmlRpcTokenType.EndParams,
      XmlNodeType.EndElement when nodeName == "param" => XmlRpcTokenType.EndParam,
      XmlNodeType.EndElement when nodeName == "fault" => XmlRpcTokenType.EndFault,
      XmlNodeType.EndElement when nodeName == "value" => XmlRpcTokenType.EndValue,
      XmlNodeType.EndElement when nodeName == "struct" => XmlRpcTokenType.EndStruct,
      XmlNodeType.EndElement when nodeName == "member" => XmlRpcTokenType.EndMember,
      XmlNodeType.EndElement when nodeName == "name" => XmlRpcTokenType.EndName,
      XmlNodeType.EndElement when nodeName == "array" => XmlRpcTokenType.EndArray,
      XmlNodeType.EndElement when nodeName == "data" => XmlRpcTokenType.EndData,
      _ => XmlRpcTokenType.Unknown,
    };
  }

  private string ReadSimpleValueNode(params string[] expectedNodeNames)
  {
    ReadOrAdvance(XmlRpcTokenType.StartValue);

    Read();
    ValidateValueNode(XmlNodeType.Element, expectedNodeNames);

    string value = ReadElementContentAsString();

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
