using System.Text;
using NUnit.Framework;
using XmlRpc.Serialization.Tests.Attributes;
using XmlRpc.Serialization.Tests.Models;

namespace XmlRpc.Serialization.Tests;

[TestFixture]
public class XmlRpcRequestDeserializationTests
{
  [Test]
  [TestCase("""
            <?xml version="1.0" encoding="UTF-8"?>
            <methodCall>
            <methodName>testMethod</methodName>
            </methodCall>
            """, "testMethod")]
  public void DeserializeValidRequestNoParamsInvokesHandler(string xml, string methodName)
  {
    bool handlerCalled = false;
    DeserializeRequestHandler handler = new DeserializeRequestHandler();

    handler.AddRequestMethod(methodName, new DeserializeRequestMethod(() => handlerCalled = true));

    byte[] data = Encoding.UTF8.GetBytes(xml);
    XmlRpcSerializer.DeserializeRequest(data, handler);

    Assert.That(handlerCalled, Is.True);
  }

  [Test]
  [TestCase<bool>("""
                  <?xml version="1.0" encoding="UTF-8"?>
                  <methodCall>
                  <methodName>testMethod1</methodName>
                  <params>
                  <param><value><boolean>1</boolean></value></param>
                  </params>
                  </methodCall>
                  """, "testMethod1", true)]
  [TestCase<bool>("""
                  <?xml version="1.0" encoding="UTF-8"?>
                  <methodCall>
                  <params>
                  <param><value><boolean>1</boolean></value></param>
                  </params>
                  <methodName>testMethod2</methodName>
                  </methodCall>
                  """, "testMethod2", true)]
  [TestCase<int>("""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <methodCall>
                 <methodName>testMethod3</methodName>
                 <params>
                 <param><value><i4>42</i4></value></param>
                 </params>
                 </methodCall>
                 """, "testMethod3", 42)]
  [TestCase<int>("""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <methodCall>
                 <methodName>testMethod4</methodName>
                 <params>
                 <param><value><int>84</int></value></param>
                 </params>
                 </methodCall>
                 """, "testMethod4", 84)]
  [TestCase<int>("""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <methodCall>
                 <params>
                 <param><value><i4>42</i4></value></param>
                 </params>
                 <methodName>testMethod5</methodName>
                 </methodCall>
                 """, "testMethod5", 42)]
  [TestCase<int>("""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <methodCall>
                 <params>
                 <param><value><int>84</int></value></param>
                 </params>
                 <methodName>testMethod6</methodName>
                 </methodCall>
                 """, "testMethod6", 84)]
  [TestCase<double>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodCall>
                    <methodName>testMethod7</methodName>
                    <params>
                    <param><value><double>-2.512345</double></value></param>
                    </params>
                    </methodCall>
                    """, "testMethod7", -2.512345)]
  [TestCase<double>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodCall>
                    <methodName>testMethod8</methodName>
                    <params>
                    <param><value><double>0.500000</double></value></param>
                    </params>
                    </methodCall>
                    """, "testMethod8", 0.500000)]
  [TestCase<double>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodCall>
                    <params>
                    <param><value><double>-2.512345</double></value></param>
                    </params>
                    <methodName>testMethod9</methodName>
                    </methodCall>
                    """, "testMethod9", -2.512345)]
  [TestCase<double>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodCall>
                    <params>
                    <param><value><double>0.500000</double></value></param>
                    </params>
                    <methodName>testMethod10</methodName>
                    </methodCall>
                    """, "testMethod10", 0.500000)]
  [TestCase<string>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodCall>
                    <methodName>testMethod11</methodName>
                    <params>
                    <param><value><string>Hello world</string></value></param>
                    </params>
                    </methodCall>
                    """, "testMethod11", "Hello world")]
  [TestCase<string>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodCall>
                    <methodName>testMethod12</methodName>
                    <params>
                    <param><value>Hello world</value></param>
                    </params>
                    </methodCall>
                    """, "testMethod12", "Hello world")]
  [TestCase<string>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodCall>
                    <params>
                    <param><value><string>Hello world</string></value></param>
                    </params>
                    <methodName>testMethod13</methodName>
                    </methodCall>
                    """, "testMethod13", "Hello world")]
  [TestCase<string>("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <methodCall>
                    <params>
                    <param><value>Hello world</value></param>
                    </params>
                    <methodName>testMethod14</methodName>
                    </methodCall>
                    """, "testMethod14", "Hello world")]
  public void DeserializeValidRequestSingleParamInvokesHandler<TParam1>(string xml, string methodName, TParam1 expectedParam1)
  {
    TParam1? value1 = default;
    DeserializeRequestHandler handler = new DeserializeRequestHandler();

    handler.AddRequestMethod(methodName, new DeserializeRequestMethod<TParam1>(param1 =>
    {
      value1 = param1;
    }));

    byte[] data = Encoding.UTF8.GetBytes(xml);
    XmlRpcSerializer.DeserializeRequest(data, handler);

    Assert.That(value1, Is.EqualTo(expectedParam1));
  }

  [Test]
  [TestCase<string, int>("""
                         <?xml version="1.0" encoding="UTF-8"?>
                         <methodCall>
                         <methodName>testMethod1</methodName>
                         <params>
                         <param><value><string>testParam1</string></value></param>
                         <param><value><i4>42</i4></value></param>
                         </params>
                         </methodCall>
                         """, "testMethod1", "testParam1", 42)]
  public void DeserializeValidRequestMultipleParamsInvokesHandler<TParam1, TParam2>(string xml, string methodName, TParam1 expectedParam1, TParam2 expectedParam2)
  {
    TParam1? value1 = default;
    TParam2? value2 = default;
    DeserializeRequestHandler handler = new DeserializeRequestHandler();

    handler.AddRequestMethod(methodName, new DeserializeRequestMethod<TParam1, TParam2>((param1, param2) =>
    {
      value1 = param1;
      value2 = param2;
    }));

    byte[] data = Encoding.UTF8.GetBytes(xml);
    XmlRpcSerializer.DeserializeRequest(data, handler);

    Assert.That(value1, Is.EqualTo(expectedParam1));
    Assert.That(value2, Is.EqualTo(expectedParam2));
  }

  [Test]
  public void DeserializeValidStructRequestMultipleParamsInvokesHandler()
  {
    const string xml = """
                       <?xml version="1.0" encoding="UTF-8"?>
                       <methodCall>
                       <methodName>TrackMania.EndRace</methodName>
                       <params>
                       <param><value><array><data>
                       <value><struct>
                       <member><name>Login</name>
                       <value><string>AAAAaAaAAa0AaAaAaaaaAA</string></value></member>
                       <member><name>NickName</name>
                       <value><string>Bla</string></value></member>
                       <member><name>PlayerId</name>
                       <value><i4>255</i4></value></member>
                       <member><name>Rank</name>
                       <value><i4>1</i4></value></member>
                       </struct></value>
                       </data></array></value></param>
                       <param><value><struct>
                       <member><name>UId</name>
                       <value><string>lNP8O0sqatiHqecUXrhH65rpQ8a</string></value></member>
                       <member><name>Name</name>
                       <value><string>Training - 03</string></value></member>
                       <member><name>FileName</name>
                       <value><string>Campaigns\Training\Training - 03.Map.Gbx</string></value></member>
                       <member><name>Author</name>
                       <value><string>Nadeo</string></value></member>
                       <member><name>AuthorNickname</name>
                       <value><string>Nadeo</string></value></member>
                       <member><name>Environnement</name>
                       <value><string>Stadium</string></value></member>
                       <member><name>Mood</name>
                       <value><string>48x48Day</string></value></member>
                       <member><name>BronzeTime</name>
                       <value><i4>13000</i4></value></member>
                       <member><name>SilverTime</name>
                       <value><i4>10000</i4></value></member>
                       <member><name>GoldTime</name>
                       <value><i4>8750</i4></value></member>
                       <member><name>AuthorTime</name>
                       <value><i4>8305</i4></value></member>
                       <member><name>CopperPrice</name>
                       <value><i4>494</i4></value></member>
                       <member><name>LapRace</name>
                       <value><boolean>0</boolean></value></member>
                       <member><name>NbLaps</name>
                       <value><i4>0</i4></value></member>
                       <member><name>NbCheckpoints</name>
                       <value><i4>1</i4></value></member>
                       <member><name>MapType</name>
                       <value><string>TrackMania\TM_Race</string></value></member>
                       <member><name>MapStyle</name>
                       <value><string></string></value></member>
                       </struct></value></param>
                       </params>
                       </methodCall>
                       """;

    List<OnEndRaceInfoRequestPlayerInfo> expectedPlayerInfos =
    [
      new OnEndRaceInfoRequestPlayerInfo
      {
        Login = "AAAAaAaAAa0AaAaAaaaaAA",
        NickName = "Bla",
        PlayerId = 255,
        Rank = 1,
      },
    ];

    OnEndRaceInfoRequestMapInfo expectedMapInfo = new OnEndRaceInfoRequestMapInfo
    {
      UId = "lNP8O0sqatiHqecUXrhH65rpQ8a",
      Name = "Training - 03",
      FileName = @"Campaigns\Training\Training - 03.Map.Gbx",
      Author = "Nadeo",
      AuthorNickname = "Nadeo",
      Environment = "Stadium",
      Mood = "48x48Day",
      BronzeTime = 13000,
      SilverTime = 10000,
      GoldTime = 8750,
      AuthorTime = 8305,
      CopperPrice = 494,
      LapRace = false,
      NbLaps = 0,
      NbCheckpoints = 1,
      MapType = "TrackMania\\TM_Race",
      MapStyle = string.Empty,
    };

    List<OnEndRaceInfoRequestPlayerInfo>? actualPlayerInfos = null;
    OnEndRaceInfoRequestMapInfo? actualMapInfo = null;

    OnEndRaceRequestHandler handler = new OnEndRaceRequestHandler((param1, param2) =>
    {
      actualPlayerInfos = param1;
      actualMapInfo = param2;
    });

    byte[] data = Encoding.UTF8.GetBytes(xml);
    XmlRpcSerializer.DeserializeRequest(data, handler);

    Assert.That(actualPlayerInfos, Is.Not.Null);
    Assert.That(actualMapInfo, Is.Not.Null);

    Assert.That(actualPlayerInfos, Has.Count.EqualTo(expectedPlayerInfos.Count));
    Assert.Multiple(() =>
    {
      Assert.That(actualPlayerInfos[0].Login, Is.EqualTo(expectedPlayerInfos[0].Login));
      Assert.That(actualPlayerInfos[0].NickName, Is.EqualTo(expectedPlayerInfos[0].NickName));
      Assert.That(actualPlayerInfos[0].PlayerId, Is.EqualTo(expectedPlayerInfos[0].PlayerId));
      Assert.That(actualPlayerInfos[0].Rank, Is.EqualTo(expectedPlayerInfos[0].Rank));
    });

    Assert.Multiple(() =>
    {
      Assert.That(actualMapInfo.UId, Is.EqualTo(expectedMapInfo.UId));
      Assert.That(actualMapInfo.Name, Is.EqualTo(expectedMapInfo.Name));
      Assert.That(actualMapInfo.FileName, Is.EqualTo(expectedMapInfo.FileName));
      Assert.That(actualMapInfo.Author, Is.EqualTo(expectedMapInfo.Author));
      Assert.That(actualMapInfo.AuthorNickname, Is.EqualTo(expectedMapInfo.AuthorNickname));
      Assert.That(actualMapInfo.Environment, Is.EqualTo(expectedMapInfo.Environment));
      Assert.That(actualMapInfo.Mood, Is.EqualTo(expectedMapInfo.Mood));
      Assert.That(actualMapInfo.BronzeTime, Is.EqualTo(expectedMapInfo.BronzeTime));
      Assert.That(actualMapInfo.SilverTime, Is.EqualTo(expectedMapInfo.SilverTime));
      Assert.That(actualMapInfo.GoldTime, Is.EqualTo(expectedMapInfo.GoldTime));
      Assert.That(actualMapInfo.AuthorTime, Is.EqualTo(expectedMapInfo.AuthorTime));
      Assert.That(actualMapInfo.CopperPrice, Is.EqualTo(expectedMapInfo.CopperPrice));
      Assert.That(actualMapInfo.LapRace, Is.EqualTo(expectedMapInfo.LapRace));
      Assert.That(actualMapInfo.NbLaps, Is.EqualTo(expectedMapInfo.NbLaps));
      Assert.That(actualMapInfo.NbCheckpoints, Is.EqualTo(expectedMapInfo.NbCheckpoints));
      Assert.That(actualMapInfo.MapType, Is.EqualTo(expectedMapInfo.MapType));
      Assert.That(actualMapInfo.MapStyle, Is.EqualTo(expectedMapInfo.MapStyle));
    });
  }

  [Test]
  [TestCase("""
            <?xml version="1.0" encoding="UTF-8"?>
            <methodCall>
            <methodName>unknownMethod</methodName>
            <params>
            <param><value><string>testParam1</string></value></param>
            <param><value><i4>42</i4></value></param>
            </params>
            </methodCall>
            """)]
  public void DeserializeUnknownRequestDoesNotThrowException(string xml)
  {
    DeserializeRequestHandler handler = new DeserializeRequestHandler();

    byte[] data = Encoding.UTF8.GetBytes(xml);
    Assert.That(() => XmlRpcSerializer.DeserializeRequest(data, handler), Throws.Nothing);
  }
}
