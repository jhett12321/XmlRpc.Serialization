using System.Text;
using NUnit.Framework;
using XmlRpc.Serialization.Tests.Attributes;
using XmlRpc.Serialization.Tests.Models;

namespace XmlRpc.Serialization.Tests;

[TestFixture]
public sealed class XmlRpcResponseSerializationTests
{
  [Test]
  [TestCase<bool>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><boolean>1</boolean></value></param></params></methodResponse>""", true)]
  [TestCase<int>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><i4>-5</i4></value></param></params></methodResponse>""", -5)]
  [TestCase<int>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><i4>1</i4></value></param></params></methodResponse>""", 1)]
  [TestCase<double>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><double>-2.512345</double></value></param></params></methodResponse>""", -2.512345D)]
  [TestCase<double>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><double>0.5</double></value></param></params></methodResponse>""", 0.5D)]
  [TestCase<string>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><string>Return an array of all available XML-RPC methods on this server.</string></value></param></params></methodResponse>""", "Return an array of all available XML-RPC methods on this server.")]
  public void SerializeValidResponseCreatesValidXml<T>(string expectedXml, T value)
  {
    byte[] serialized = XmlRpcSerializer.SerializeResponse(value);
    Assert.That(Encoding.UTF8.GetString(serialized), Is.EqualTo(expectedXml));
  }

  [Test]
  [TestCase<sbyte>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><i4>-128</i4></value></param></params></methodResponse>""", sbyte.MinValue)]
  [TestCase<byte>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><i4>255</i4></value></param></params></methodResponse>""", byte.MaxValue)]
  [TestCase<short>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><i4>-32768</i4></value></param></params></methodResponse>""", short.MinValue)]
  [TestCase<ushort>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><i4>65535</i4></value></param></params></methodResponse>""", ushort.MaxValue)]
  [TestCase<float>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><double>10.5</double></value></param></params></methodResponse>""", 10.5f)]
  [TestCase<char>("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><string>B</string></value></param></params></methodResponse>""", 'B')]
  public void SerializeValidResponseExtendedCreatesValidXml<T>(string expectedXml, T value)
  {
    byte[] serialized = XmlRpcSerializer.SerializeResponse(value);
    Assert.That(Encoding.UTF8.GetString(serialized), Is.EqualTo(expectedXml));
  }

  [Test]
  public void SerializeValidStructResponseCreatesValidXml()
  {
    const string expectedXml = """<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><struct><member><name>Code</name><value><i4>4</i4></value></member><member><name>Name</name><value><string>Running - Play</string></value></member></struct></value></param></params></methodResponse>""";

    ServerStatusResponse expected = new ServerStatusResponse
    {
      Code = 4,
      Name = "Running - Play",
    };

    byte[] serialized = XmlRpcSerializer.SerializeResponse(expected, ServerStatusResponseContext.ServerStatusResponse);
    Assert.That(Encoding.UTF8.GetString(serialized), Is.EqualTo(expectedXml));
  }
}
