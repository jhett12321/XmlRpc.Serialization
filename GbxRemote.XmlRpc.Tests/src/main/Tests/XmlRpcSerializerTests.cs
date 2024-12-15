using System.Text;
using GbxRemote.XmlRpc.Models;
using GbxRemote.XmlRpc.Serialization;
using NUnit.Framework;

namespace GbxRemote.XmlRpc.Tests.Tests;

public sealed class XmlRpcSerializerTests
{
  [Test]
  [TestCase("""<?xml version="1.0" encoding="utf-8"?><methodCall><methodName>Authenticate</methodName><params><param><value>SuperAdmin</value></param><param><value>SuperAdmin</value></param></params></methodCall>""",
    0x80000000, "Authenticate", "SuperAdmin", "SuperAdmin")]
  [TestCase("""<?xml version="1.0" encoding="utf-8"?><methodCall><methodName>system.listMethods</methodName></methodCall>""",
    0x80000001, "system.listMethods")]
  public async Task SerializeRequestTest(string expected, uint requestId, string methodName, params object[] args)
  {
    XmlRpcSerializer serializer = new XmlRpcSerializer();
    XmlRpcRequestMessage requestMessage = new XmlRpcRequestMessage(methodName, args);

    using MemoryStream stream = new MemoryStream();
    await serializer.SerializeAsync(stream, requestMessage, requestId);

    stream.Position = 0;
    using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
    {
      Assert.That(reader.ReadInt32(), Is.EqualTo(expected.Length));
      Assert.That(reader.ReadUInt32(), Is.EqualTo(requestId));
    }

    using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
    Assert.That(await streamReader.ReadToEndAsync(), Is.EqualTo(expected));
  }
}
