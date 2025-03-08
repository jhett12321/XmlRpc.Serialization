using System.Text;
using NUnit.Framework;
using XmlRpc.Serialization.Tests.Models;

namespace XmlRpc.Serialization.Tests;

[TestFixture]
public sealed class XmlRpcRequestSerializationTests
{
  [Test]
  [TestCase("""<?xml version="1.0" encoding="UTF-8"?><methodCall><methodName>Authenticate</methodName><params><param><value><string>SuperAdmin</string></value></param><param><value><string>SuperAdmin</string></value></param></params></methodCall>""",
    "Authenticate", "SuperAdmin", "SuperAdmin")]
  [TestCase("""<?xml version="1.0" encoding="UTF-8"?><methodCall><methodName>system.listMethods</methodName></methodCall>""",
    "system.listMethods")]
  public void SerializeValidRequestCreatesValidXml(string expected, string methodName, params string[] args)
  {
    XmlRpcRequestMessage requestMessage = new XmlRpcRequestMessage(methodName);
    foreach (string arg in args)
    {
      requestMessage.Param(arg);
    }

    byte[] serialized = XmlRpcSerializer.SerializeRequest(requestMessage);
    Assert.That(Encoding.UTF8.GetString(serialized), Is.EqualTo(expected));
  }

  [Test]
  public void SerializeValidRequestStructCreatesValidXml()
  {
    const string expected = """<?xml version="1.0" encoding="UTF-8"?><methodCall><methodName>TrackMania.PlayerInfoChanged</methodName><params><param><value><struct><member><name>Login</name><value><string>AAAAaAaAAa0AaAaAaaaaAA</string></value></member><member><name>NickName</name><value><string>Bla</string></value></member><member><name>PlayerId</name><value><i4>236</i4></value></member><member><name>TeamId</name><value><i4>-1</i4></value></member><member><name>SpectatorStatus</name><value><i4>0</i4></value></member><member><name>LadderRanking</name><value><i4>-1</i4></value></member><member><name>Flags</name><value><i4>101000000</i4></value></member><member><name>LadderScore</name><value><double>0.000001</double></value></member></struct></value></param></params></methodCall>""";
    OnPlayerInfoChangeRequest request = new OnPlayerInfoChangeRequest
    {
      Login = "AAAAaAaAAa0AaAaAaaaaAA",
      NickName = "Bla",
      PlayerId = 236,
      TeamId = -1,
      SpectatorStatus = 0,
      LadderRanking = -1,
      Flags = 101000000,
      LadderScore = 0.000001,
    };

    XmlRpcRequestMessage requestMessage = new XmlRpcRequestMessage("TrackMania.PlayerInfoChanged")
      .Param(request, OnPlayerInfoChangeRequestContext.OnPlayerInfoChangeRequest);

    byte[] serialized = XmlRpcSerializer.SerializeRequest(requestMessage);
    Assert.That(Encoding.UTF8.GetString(serialized), Is.EqualTo(expected));
  }
}
