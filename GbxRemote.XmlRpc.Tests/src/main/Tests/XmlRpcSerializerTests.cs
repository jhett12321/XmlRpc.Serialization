using System.Text;
using GbxRemote.XmlRpc.Serialization;
using GbxRemote.XmlRpc.Serialization.Exceptions;
using GbxRemote.XmlRpc.Serialization.Models;
using GbxRemote.XmlRpc.Tests.Attributes;
using GbxRemote.XmlRpc.Tests.Models;
using NUnit.Framework;

namespace GbxRemote.XmlRpc.Tests;

[TestFixture]
public sealed class XmlRpcSerializerTests
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

  [Test]
  [TestCase<bool>("""
                  <?xml version="1.0" encoding="UTF-8"?>
                  <methodResponse>
                  <params>
                  <param><value><boolean>1</boolean></value></param>
                  </params>
                  </methodResponse>
                  """, true)]
  [TestCase<int>("""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <methodResponse>
                 <params>
                 <param><value><i4>-5</i4></value></param>
                 </params>
                 </methodResponse>
                 """, -5)]
  [TestCase<int>("""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <methodResponse>
                 <params>
                 <param><value><int>1</int></value></param>
                 </params>
                 </methodResponse>
                 """, 1)]
  [TestCase<double>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodResponse>
                    <params>
                    <param><value><double>-2.512345</double></value></param>
                    </params>
                    </methodResponse>
                    """, -2.512345D)]
  [TestCase<double>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodResponse>
                    <params>
                    <param><value><double>0.500000</double></value></param>
                    </params>
                    </methodResponse>
                    """, 0.5D)]
  [TestCase<string>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodResponse>
                    <params>
                    <param><value><string>Return an array of all available XML-RPC methods on this server.</string></value></param>
                    </params>
                    </methodResponse>
                    """, "Return an array of all available XML-RPC methods on this server.")]
  [TestCase<string>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodResponse>
                    <params>
                    <param><value>Return an array of all available XML-RPC methods on this server.</value></param>
                    </params>
                    </methodResponse>
                    """, "Return an array of all available XML-RPC methods on this server.")]
  public void DeserializeValidResponseCreatesValidValue<T>(string xml, object value)
  {
    byte[] data = Encoding.UTF8.GetBytes(xml);

    T response = XmlRpcSerializer.DeserializeResponse<T>(data);
    Assert.That(response, Is.EqualTo(value));
  }

  [Test]
  [TestCase("""
            <?xml version="1.0" encoding="UTF-8"?>
            <methodResponse>
            <params>
            <param><value><base64>eW91IGNhbid0IHJlYWQgdGhpcyE=</base64></value></param>
            </params>
            </methodResponse>
            """, "eW91IGNhbid0IHJlYWQgdGhpcyE=")]
  public void DeserializeValidBase64ResponseCreatesValidValue(string xml, string expected)
  {
    byte[] data = Encoding.UTF8.GetBytes(xml);
    byte[] response = XmlRpcSerializer.DeserializeResponse<byte[]>(data);
    string base64Response = Convert.ToBase64String(response);

    Assert.That(base64Response, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("""
            <?xml version="1.0" encoding="UTF-8"?>
            <methodResponse>
            <params>
            <param><value><dateTime.iso8601>19980717T14:08:55</dateTime.iso8601></value></param>
            </params>
            </methodResponse>
            """, 630362813350000000)] // 1998-07-17 14:08:55
  public void DeserializeValidDateTimeResponseCreatesValidValue(string xml, long expectedTicks)
  {
    DateTime expectedDateTime = new DateTime(expectedTicks, DateTimeKind.Utc);

    byte[] data = Encoding.UTF8.GetBytes(xml);
    DateTime response = XmlRpcSerializer.DeserializeResponse<DateTime>(data);

    Assert.That(response, Is.EqualTo(expectedDateTime));
  }

  [Test]
  [TestCase("""
            <?xml version="1.0" encoding="UTF-8"?>
            <methodResponse>
            <params>
            <param><value><array><data>
            <value><string>system.listMethods</string></value>
            <value><string>system.methodSignature</string></value>
            <value><string>system.methodHelp</string></value>
            </data></array></value></param>
            </params>
            </methodResponse>
            """, "system.listMethods", "system.methodSignature", "system.methodHelp")]
  [TestCase("""
            <?xml version="1.0" encoding="UTF-8"?>
            <methodResponse>
            <params>
            <param><value><array><data></data></array></value></param>
            </params>
            </methodResponse>
            """)]
  public void DeserializeValidArrayResponseCreatesValidValue(string xml, params object[] values)
  {
    byte[] data = Encoding.UTF8.GetBytes(xml);

    List<string> response = XmlRpcSerializer.DeserializeResponse<List<string>>(data);
    Assert.That(response, Is.EquivalentTo(values));
  }

  [Test]
  public void DeserializeValidStructResponseCreatesValidValue()
  {
    const string xml = """
                       <?xml version="1.0" encoding="UTF-8"?>
                       <methodResponse>
                       <params>
                       <param><value><struct>
                       <member><name>Code</name>
                       <value><i4>4</i4></value></member>
                       <member><name>Name</name>
                       <value><string>Running - Play</string></value></member>
                       </struct></value></param>
                       </params>
                       </methodResponse>
                       """;

    ServerStatusResponse expected = new ServerStatusResponse();
    expected.Code = 4;
    expected.Name = "Running - Play";

    byte[] data = Encoding.UTF8.GetBytes(xml);

    ServerStatusResponse response = XmlRpcSerializer.DeserializeResponse<ServerStatusResponse>(data, ServerStatusResponseContext.ServerStatusResponse);

    Assert.That(response.Code, Is.EqualTo(expected.Code));
    Assert.That(response.Name, Is.EqualTo(expected.Name));
  }

  [Test]
  public void DeserializeNestedStructResponseCreatesValidValue()
  {
    const string xml = """
                       <?xml version="1.0" encoding="UTF-8"?>
                       <methodResponse>
                       <params>
                       <param><value><struct>
                       <member><name>Uptime</name>
                       <value><i4>10279</i4></value></member>
                       <member><name>NbrConnection</name>
                       <value><i4>1</i4></value></member>
                       <member><name>MeanConnectionTime</name>
                       <value><i4>78</i4></value></member>
                       <member><name>MeanNbrPlayer</name>
                       <value><i4>0</i4></value></member>
                       <member><name>RecvNetRate</name>
                       <value><i4>202</i4></value></member>
                       <member><name>SendNetRate</name>
                       <value><i4>65</i4></value></member>
                       <member><name>TotalReceivingSize</name>
                       <value><i4>18852</i4></value></member>
                       <member><name>TotalSendingSize</name>
                       <value><i4>5169</i4></value></member>
                       <member><name>PlayerNetInfos</name>
                       <value><array><data>
                       <value><struct>
                       <member><name>Login</name>
                       <value><string>AAAAaAaAAa0AaAaAaaaaAA</string></value></member>
                       <member><name>IPAddress</name>
                       <value><string>127.0.0.1</string></value></member>
                       <member><name>StateUpdateLatency</name>
                       <value><i4>0</i4></value></member>
                       <member><name>StateUpdatePeriod</name>
                       <value><i4>0</i4></value></member>
                       <member><name>LatestNetworkActivity</name>
                       <value><i4>0</i4></value></member>
                       <member><name>PacketLossRate</name>
                       <value><double>0.001276</double></value></member>
                       </struct></value>
                       </data></array></value></member>
                       </struct></value></param>
                       </params>
                       </methodResponse>
                       """;

    PlayerInfoResponse expected = new PlayerInfoResponse();
    expected.Uptime = 10279;
    expected.NbrConnection = 1;
    expected.MeanConnectionTime = 78;
    expected.MeanNbrPlayer = 0;
    expected.RecvNetRate = 202;
    expected.SendNetRate = 65;
    expected.TotalReceivingSize = 18852;
    expected.TotalSendingSize = 5169;
    expected.PlayerNetInfos = new List<PlayerNetInfo>();

    PlayerNetInfo expectedPlayerInfo = new PlayerNetInfo();
    expectedPlayerInfo.Login = "AAAAaAaAAa0AaAaAaaaaAA";
    expectedPlayerInfo.IPAddress = "127.0.0.1";
    expectedPlayerInfo.StateUpdateLatency = 0;
    expectedPlayerInfo.StateUpdatePeriod = 0;
    expectedPlayerInfo.LatestNetworkActivity = 0;
    expectedPlayerInfo.PacketLossRate = 0.001276D;

    expected.PlayerNetInfos.Add(expectedPlayerInfo);

    byte[] data = Encoding.UTF8.GetBytes(xml);

    PlayerInfoResponse response = XmlRpcSerializer.DeserializeResponse<PlayerInfoResponse>(data, PlayerInfoResponseContext.PlayerInfoResponse);

    Assert.Multiple(() =>
    {
      Assert.That(response.Uptime, Is.EqualTo(expected.Uptime));
      Assert.That(response.NbrConnection, Is.EqualTo(expected.NbrConnection));
      Assert.That(response.MeanConnectionTime, Is.EqualTo(expected.MeanConnectionTime));
      Assert.That(response.MeanNbrPlayer, Is.EqualTo(expected.MeanNbrPlayer));
      Assert.That(response.RecvNetRate, Is.EqualTo(expected.RecvNetRate));
      Assert.That(response.SendNetRate, Is.EqualTo(expected.SendNetRate));
      Assert.That(response.TotalReceivingSize, Is.EqualTo(expected.TotalReceivingSize));
      Assert.That(response.TotalSendingSize, Is.EqualTo(expected.TotalSendingSize));
      Assert.That(response.PlayerNetInfos.Count, Is.EqualTo(expected.PlayerNetInfos.Count));
    });

    PlayerNetInfo responsePlayerInfo = response.PlayerNetInfos[0];

    Assert.Multiple(() =>
    {
      Assert.That(responsePlayerInfo.Login, Is.EqualTo(expectedPlayerInfo.Login));
      Assert.That(responsePlayerInfo.IPAddress, Is.EqualTo(expectedPlayerInfo.IPAddress));
      Assert.That(responsePlayerInfo.StateUpdateLatency, Is.EqualTo(expectedPlayerInfo.StateUpdateLatency));
      Assert.That(responsePlayerInfo.StateUpdatePeriod, Is.EqualTo(expectedPlayerInfo.StateUpdatePeriod));
      Assert.That(responsePlayerInfo.LatestNetworkActivity, Is.EqualTo(expectedPlayerInfo.LatestNetworkActivity));
      Assert.That(responsePlayerInfo.PacketLossRate, Is.EqualTo(expectedPlayerInfo.PacketLossRate));
    });
  }

  [Test]
  public void DeserializeMultipleNestedStructResponseCreatesValidValue()
  {
    const string xml = """
                       <?xml version="1.0" encoding="UTF-8"?>
                       <methodResponse>
                       <params>
                       <param><value><struct>
                       <member><name>Uptime</name>
                       <value><i4>10279</i4></value></member>
                       <member><name>NbrConnection</name>
                       <value><i4>1</i4></value></member>
                       <member><name>MeanConnectionTime</name>
                       <value><i4>78</i4></value></member>
                       <member><name>MeanNbrPlayer</name>
                       <value><i4>0</i4></value></member>
                       <member><name>RecvNetRate</name>
                       <value><i4>202</i4></value></member>
                       <member><name>SendNetRate</name>
                       <value><i4>65</i4></value></member>
                       <member><name>TotalReceivingSize</name>
                       <value><i4>18852</i4></value></member>
                       <member><name>TotalSendingSize</name>
                       <value><i4>5169</i4></value></member>
                       <member><name>PlayerNetInfos</name>
                       <value><array><data>
                       <value><struct>
                       <member><name>Login</name>
                       <value><string>AAAAaAaAAa0AaAaAaaaaAA</string></value></member>
                       <member><name>IPAddress</name>
                       <value><string>127.0.0.1</string></value></member>
                       <member><name>StateUpdateLatency</name>
                       <value><i4>0</i4></value></member>
                       <member><name>StateUpdatePeriod</name>
                       <value><i4>0</i4></value></member>
                       <member><name>LatestNetworkActivity</name>
                       <value><i4>0</i4></value></member>
                       <member><name>PacketLossRate</name>
                       <value><double>0.001276</double></value></member>
                       </struct></value>
                       <value><struct>
                       <member><name>Login</name>
                       <value><string>BBBBbBbBBb0BbBbBbbbbBB</string></value></member>
                       <member><name>IPAddress</name>
                       <value><string>192.168.1.1</string></value></member>
                       <member><name>StateUpdateLatency</name>
                       <value><i4>0</i4></value></member>
                       <member><name>StateUpdatePeriod</name>
                       <value><i4>0</i4></value></member>
                       <member><name>LatestNetworkActivity</name>
                       <value><i4>0</i4></value></member>
                       <member><name>PacketLossRate</name>
                       <value><double>0.001576</double></value></member>
                       </struct></value>
                       </data></array></value></member>
                       </struct></value></param>
                       </params>
                       </methodResponse>
                       """;

    PlayerInfoResponse expected = new PlayerInfoResponse();
    expected.Uptime = 10279;
    expected.NbrConnection = 1;
    expected.MeanConnectionTime = 78;
    expected.MeanNbrPlayer = 0;
    expected.RecvNetRate = 202;
    expected.SendNetRate = 65;
    expected.TotalReceivingSize = 18852;
    expected.TotalSendingSize = 5169;
    expected.PlayerNetInfos = new List<PlayerNetInfo>();

    PlayerNetInfo expectedNetInfo1 = new PlayerNetInfo();
    expectedNetInfo1.Login = "AAAAaAaAAa0AaAaAaaaaAA";
    expectedNetInfo1.IPAddress = "127.0.0.1";
    expectedNetInfo1.StateUpdateLatency = 0;
    expectedNetInfo1.StateUpdatePeriod = 0;
    expectedNetInfo1.LatestNetworkActivity = 0;
    expectedNetInfo1.PacketLossRate = 0.001276D;

    PlayerNetInfo expectedNetInfo2 = new PlayerNetInfo();
    expectedNetInfo2.Login = "BBBBbBbBBb0BbBbBbbbbBB";
    expectedNetInfo2.IPAddress = "192.168.1.1";
    expectedNetInfo2.StateUpdateLatency = 0;
    expectedNetInfo2.StateUpdatePeriod = 0;
    expectedNetInfo2.LatestNetworkActivity = 0;
    expectedNetInfo2.PacketLossRate = 0.001576D;

    expected.PlayerNetInfos.Add(expectedNetInfo1);
    expected.PlayerNetInfos.Add(expectedNetInfo2);

    byte[] data = Encoding.UTF8.GetBytes(xml);

    PlayerInfoResponse response = XmlRpcSerializer.DeserializeResponse<PlayerInfoResponse>(data, PlayerInfoResponseContext.PlayerInfoResponse);

    Assert.Multiple(() =>
    {
      Assert.That(response.Uptime, Is.EqualTo(expected.Uptime));
      Assert.That(response.NbrConnection, Is.EqualTo(expected.NbrConnection));
      Assert.That(response.MeanConnectionTime, Is.EqualTo(expected.MeanConnectionTime));
      Assert.That(response.MeanNbrPlayer, Is.EqualTo(expected.MeanNbrPlayer));
      Assert.That(response.RecvNetRate, Is.EqualTo(expected.RecvNetRate));
      Assert.That(response.SendNetRate, Is.EqualTo(expected.SendNetRate));
      Assert.That(response.TotalReceivingSize, Is.EqualTo(expected.TotalReceivingSize));
      Assert.That(response.TotalSendingSize, Is.EqualTo(expected.TotalSendingSize));
      Assert.That(response.PlayerNetInfos.Count, Is.EqualTo(expected.PlayerNetInfos.Count));
    });

    AssertNetInfo(response.PlayerNetInfos[0], expectedNetInfo1);
    AssertNetInfo(response.PlayerNetInfos[1], expectedNetInfo2);

    void AssertNetInfo(PlayerNetInfo actual, PlayerNetInfo expected)
    {
      Assert.Multiple(() =>
      {
        Assert.That(actual.Login, Is.EqualTo(expected.Login));
        Assert.That(actual.IPAddress, Is.EqualTo(expected.IPAddress));
        Assert.That(actual.StateUpdateLatency, Is.EqualTo(expected.StateUpdateLatency));
        Assert.That(actual.StateUpdatePeriod, Is.EqualTo(expected.StateUpdatePeriod));
        Assert.That(actual.LatestNetworkActivity, Is.EqualTo(expected.LatestNetworkActivity));
        Assert.That(actual.PacketLossRate, Is.EqualTo(expected.PacketLossRate));
      });
    }
  }

  [Test]
  [TestCase("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><fault><value><struct><member><name>faultCode</name><value><int>-1000</int></value></member><member><name>faultString</name><value><string>Not Supported.</string></value></member></struct></value></fault></methodResponse>""",
    -1000, "Not Supported.")]
  [TestCase("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><fault><value><struct><member><value><int>-1000</int></value><name>faultCode</name></member><member><value><string>Not Supported.</string></value><name>faultString</name></member></struct></value></fault></methodResponse>""",
    -1000, "Not Supported.")]
  public void DeserializeFaultResponseThrowsException(string xml, int faultCode, string message)
  {
    byte[] data = Encoding.UTF8.GetBytes(xml);

    Assert.That(() =>
    {
      XmlRpcSerializer.DeserializeResponse<string>(data);
    }, Throws.Exception.TypeOf<XmlRpcFaultException>().And.Message.EqualTo($"Received fault response: ({faultCode}) {message}"));
  }

  [Test]
  [TestCase("")]
  [TestCase("""xml version="1.0" encoding="UTF-8"?>""")]
  public void DeserializeInvalidXmlThrowsSerializationException(string xml)
  {
    byte[] data = Encoding.UTF8.GetBytes(xml);

    Assert.That(() =>
    {
      XmlRpcSerializer.DeserializeResponse<string>(data);
    }, Throws.Exception.TypeOf<XmlRpcSerializationException>());
  }

  [Test]
  [TestCase<int>("""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <methodResponse>
                 <params>
                 <param><value><boolean>1</boolean></value></param>
                 </params>
                 </methodResponse>
                 """, true)]
  [TestCase<bool>("""
                  <?xml version="1.0" encoding="UTF-8"?>
                  <methodResponse>
                  <params>
                  <param><value><i4>-5</i4></value></param>
                  </params>
                  </methodResponse>
                  """, -5)]
  [TestCase<double>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodResponse>
                    <params>
                    <param><value><int>1</int></value></param>
                    </params>
                    </methodResponse>
                    """, 1)]
  [TestCase<string>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodResponse>
                    <params>
                    <param><value><double>-2.512345</double></value></param>
                    </params>
                    </methodResponse>
                    """, -2.512345D)]
  [TestCase<int>("""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <methodResponse>
                 <params>
                 <param><value><double>0.500000</double></value></param>
                 </params>
                 </methodResponse>
                 """, 0.5D)]
  [TestCase<byte[]>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodResponse>
                    <params>
                    <param><value><string>Return an array of all available XML-RPC methods on this server.</string></value></param>
                    </params>
                    </methodResponse>
                    """, "Return an array of all available XML-RPC methods on this server.")]
  public void DeserializeWrongTypeThrowsSerializationException<T>(string xml, object value)
  {
    byte[] data = Encoding.UTF8.GetBytes(xml);

    Assert.That(() =>
    {
      XmlRpcSerializer.DeserializeResponse<T>(data);
    }, Throws.Exception.TypeOf<XmlRpcSerializationException>());
  }

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
  public void SerializeValidStructResponseCreatesValidXml()
  {
    const string expectedXml = """<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value><struct><member><name>Code</name><value><i4>4</i4></value></member><member><name>Name</name><value><string>Running - Play</string></value></member></struct></value></param></params></methodResponse>""";

    ServerStatusResponse expected = new ServerStatusResponse();
    expected.Code = 4;
    expected.Name = "Running - Play";

    byte[] serialized = XmlRpcSerializer.SerializeResponse(expected, ServerStatusResponseContext.ServerStatusResponse);
    Assert.That(Encoding.UTF8.GetString(serialized), Is.EqualTo(expectedXml));
  }
}
