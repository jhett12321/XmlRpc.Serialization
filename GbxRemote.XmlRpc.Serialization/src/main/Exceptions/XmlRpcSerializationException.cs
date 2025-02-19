using System;

namespace GbxRemote.XmlRpc.Serialization.Exceptions;

public class XmlRpcSerializationException(string message) : Exception(message);
