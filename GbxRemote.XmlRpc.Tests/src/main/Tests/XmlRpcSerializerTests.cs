using System.Dynamic;
using System.Runtime.Serialization;
using System.Text;
using GbxRemote.XmlRpc.Client;
using GbxRemote.XmlRpc.Serialization;
using GbxRemote.XmlRpc.Serialization.Models;
using GbxRemote.XmlRpc.Tests.Attributes;
using NUnit.Framework;

namespace GbxRemote.XmlRpc.Tests.Tests;

[TestFixture]
public sealed class XmlRpcSerializerTests
{
  [Test]
  [TestCase("""<?xml version="1.0" encoding="utf-8"?><methodCall><methodName>Authenticate</methodName><params><param><value>SuperAdmin</value></param><param><value>SuperAdmin</value></param></params></methodCall>""",
    "Authenticate", "SuperAdmin", "SuperAdmin")]
  [TestCase("""<?xml version="1.0" encoding="utf-8"?><methodCall><methodName>system.listMethods</methodName></methodCall>""",
    "system.listMethods")]
  public async Task SerializeValidRequestCreatesValidXml(string expected, string methodName, params object[] args)
  {
    XmlRpcSerializer serializer = new XmlRpcSerializer();
    XmlRpcRequestMessage requestMessage = new XmlRpcRequestMessage(methodName, args);

    byte[] serialized = await serializer.SerializeAsync(requestMessage);
    Assert.That(Encoding.UTF8.GetString(serialized), Is.EqualTo(expected));
  }

  [Test]
  [TestCase<XmlRpcResponseBoolean>("""
                                   <?xml version="1.0" encoding="UTF-8"?>
                                   <methodResponse>
                                   <params>
                                   <param><value><boolean>1</boolean></value></param>
                                   </params>
                                   </methodResponse>
                                   """, true)]
  [TestCase<XmlRpcResponseInt32>("""
                                   <?xml version="1.0" encoding="UTF-8"?>
                                   <methodResponse>
                                   <params>
                                   <param><value><i4>-5</i4></value></param>
                                   </params>
                                   </methodResponse>
                                   """, -5)]
  [TestCase<XmlRpcResponseInt32>("""
                                 <?xml version="1.0" encoding="UTF-8"?>
                                 <methodResponse>
                                 <params>
                                 <param><value><int>1</int></value></param>
                                 </params>
                                 </methodResponse>
                                 """, 1)]
  [TestCase<XmlRpcResponseDouble>("""
                                 <?xml version="1.0" encoding="UTF-8"?>
                                 <methodResponse>
                                 <params>
                                 <param><value><double>-2.512345</double></value></param>
                                 </params>
                                 </methodResponse>
                                 """, -2.512345D)]
  [TestCase<XmlRpcResponseDouble>("""
                                 <?xml version="1.0" encoding="UTF-8"?>
                                 <methodResponse>
                                 <params>
                                 <param><value><double>0.500000</double></value></param>
                                 </params>
                                 </methodResponse>
                                 """, 0.5D)]
  [TestCase<XmlRpcResponseString>("""
                                 <?xml version="1.0" encoding="UTF-8"?>
                                 <methodResponse>
                                 <params>
                                 <param><value><string>Return an array of all available XML-RPC methods on this server.</string></value></param>
                                 </params>
                                 </methodResponse>
                                 """, "Return an array of all available XML-RPC methods on this server.")]
  public void DeserializeValidResponseCreatesValidValue<T>(string xml, object value) where T : IXmlRpcResponseValue, new()
  {
    XmlRpcSerializer serializer = new XmlRpcSerializer();
    byte[] data = Encoding.UTF8.GetBytes(xml);

    T response = serializer.Deserialize<T>(data);
    Assert.That(response.Value, Is.EqualTo(value));
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
    XmlRpcSerializer serializer = new XmlRpcSerializer();
    byte[] data = Encoding.UTF8.GetBytes(xml);

    XmlRpcResponseArray response = serializer.Deserialize<XmlRpcResponseArray>(data);
    Assert.That(response.Value, Is.EquivalentTo(values));
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

    dynamic expected = new ExpandoObject();
    expected.Code = 4;
    expected.Name = "Running - Play";

    XmlRpcSerializer serializer = new XmlRpcSerializer();
    byte[] data = Encoding.UTF8.GetBytes(xml);

    XmlRpcResponseStruct responseStruct = serializer.Deserialize<XmlRpcResponseStruct>(data);
    dynamic response = responseStruct.Value!;

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

    dynamic expected = new ExpandoObject();
    expected.Uptime = 10279;
    expected.NbrConnection = 1;
    expected.MeanConnectionTime = 78;
    expected.MeanNbrPlayer = 0;
    expected.RecvNetRate = 202;
    expected.SendNetRate = 65;
    expected.TotalReceivingSize = 18852;
    expected.TotalSendingSize = 5169;
    expected.PlayerNetInfos = new List<dynamic>();

    dynamic expectedPlayerInfo = new ExpandoObject();
    expectedPlayerInfo.Login = "AAAAaAaAAa0AaAaAaaaaAA";
    expectedPlayerInfo.IPAddress = "127.0.0.1";
    expectedPlayerInfo.StateUpdateLatency = 0;
    expectedPlayerInfo.StateUpdatePeriod = 0;
    expectedPlayerInfo.LatestNetworkActivity = 0;
    expectedPlayerInfo.PacketLossRate = 0.001276D;

    expected.PlayerNetInfos.Add(expectedPlayerInfo);

    XmlRpcSerializer serializer = new XmlRpcSerializer();
    byte[] data = Encoding.UTF8.GetBytes(xml);

    XmlRpcResponseStruct responseStruct = serializer.Deserialize<XmlRpcResponseStruct>(data);
    dynamic response = responseStruct.Value!;

    Assert.That(response.Uptime, Is.EqualTo(expected.Uptime));
    Assert.That(response.NbrConnection, Is.EqualTo(expected.NbrConnection));
    Assert.That(response.MeanConnectionTime, Is.EqualTo(expected.MeanConnectionTime));
    Assert.That(response.MeanNbrPlayer, Is.EqualTo(expected.MeanNbrPlayer));
    Assert.That(response.RecvNetRate, Is.EqualTo(expected.RecvNetRate));
    Assert.That(response.SendNetRate, Is.EqualTo(expected.SendNetRate));
    Assert.That(response.TotalReceivingSize, Is.EqualTo(expected.TotalReceivingSize));
    Assert.That(response.TotalSendingSize, Is.EqualTo(expected.TotalSendingSize));
    Assert.That(response.PlayerNetInfos.Count, Is.EqualTo(expected.PlayerNetInfos.Count));

    dynamic responsePlayerInfo = response.PlayerNetInfos[0];
    Assert.That(responsePlayerInfo.Login, Is.EqualTo(expectedPlayerInfo.Login));
    Assert.That(responsePlayerInfo.IPAddress, Is.EqualTo(expectedPlayerInfo.IPAddress));
    Assert.That(responsePlayerInfo.StateUpdateLatency, Is.EqualTo(expectedPlayerInfo.StateUpdateLatency));
    Assert.That(responsePlayerInfo.StateUpdatePeriod, Is.EqualTo(expectedPlayerInfo.StateUpdatePeriod));
    Assert.That(responsePlayerInfo.LatestNetworkActivity, Is.EqualTo(expectedPlayerInfo.LatestNetworkActivity));
    Assert.That(responsePlayerInfo.PacketLossRate, Is.EqualTo(expectedPlayerInfo.PacketLossRate));
  }

  [Test]
  [TestCase("""<?xml version="1.0" encoding="UTF-8"?><methodResponse><fault><value><struct><member><name>faultCode</name><value><int>-1000</int></value></member><member><name>faultString</name><value><string>Not Supported.</string></value></member></struct></value></fault></methodResponse>""",
    -1000, "Not Supported.")]
  public void DeserializeFaultResponseThrowsException(string xml, int faultCode, string message)
  {
    XmlRpcSerializer serializer = new XmlRpcSerializer();
    byte[] data = Encoding.UTF8.GetBytes(xml);

    Assert.That(() =>
    {
      serializer.Deserialize<XmlRpcResponseRaw>(data);
    }, Throws.Exception.TypeOf<XmlRpcFaultException>().And.Message.EqualTo($"Received fault response: ({faultCode}) {message}"));
  }

  [Test]
  [TestCase("")]
  [TestCase("""xml version="1.0" encoding="UTF-8"?>""")]
  public void DeserializeInvalidXmlThrowsSerializationException(string xml)
  {
    XmlRpcSerializer serializer = new XmlRpcSerializer();
    byte[] data = Encoding.UTF8.GetBytes(xml);

    Assert.That(() =>
    {
      serializer.Deserialize<XmlRpcResponseRaw>(data);
    }, Throws.Exception.TypeOf<SerializationException>());
  }

  [Test]
  [TestCase<XmlRpcResponseInt32>("""
                                 <?xml version="1.0" encoding="UTF-8"?>
                                 <methodResponse>
                                 <params>
                                 <param><value><boolean>1</boolean></value></param>
                                 </params>
                                 </methodResponse>
                                 """, true)]
  [TestCase<XmlRpcResponseBoolean>("""
                                   <?xml version="1.0" encoding="UTF-8"?>
                                   <methodResponse>
                                   <params>
                                   <param><value><i4>-5</i4></value></param>
                                   </params>
                                   </methodResponse>
                                   """, -5)]
  [TestCase<XmlRpcResponseDouble>("""
                                  <?xml version="1.0" encoding="UTF-8"?>
                                  <methodResponse>
                                  <params>
                                  <param><value><int>1</int></value></param>
                                  </params>
                                  </methodResponse>
                                  """, 1)]
  [TestCase<XmlRpcResponseString>("""
                                  <?xml version="1.0" encoding="UTF-8"?>
                                  <methodResponse>
                                  <params>
                                  <param><value><double>-2.512345</double></value></param>
                                  </params>
                                  </methodResponse>
                                  """, -2.512345D)]
  [TestCase<XmlRpcResponseInt32>("""
                                 <?xml version="1.0" encoding="UTF-8"?>
                                 <methodResponse>
                                 <params>
                                 <param><value><double>0.500000</double></value></param>
                                 </params>
                                 </methodResponse>
                                 """, 0.5D)]
  [TestCase<XmlRpcResponseStruct>("""
                                 <?xml version="1.0" encoding="UTF-8"?>
                                 <methodResponse>
                                 <params>
                                 <param><value><string>Return an array of all available XML-RPC methods on this server.</string></value></param>
                                 </params>
                                 </methodResponse>
                                 """, "Return an array of all available XML-RPC methods on this server.")]
  public void DeserializeWrongTypeThrowsSerializationException<T>(string xml, object value) where T : IXmlRpcResponseValue, new()
  {
    XmlRpcSerializer serializer = new XmlRpcSerializer();
    byte[] data = Encoding.UTF8.GetBytes(xml);

    Assert.That(() =>
    {
      serializer.Deserialize<T>(data);
    }, Throws.Exception.TypeOf<SerializationException>());
  }
}
