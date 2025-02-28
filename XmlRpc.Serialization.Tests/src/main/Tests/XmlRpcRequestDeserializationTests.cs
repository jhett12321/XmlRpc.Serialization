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
