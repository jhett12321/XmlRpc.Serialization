using System;

namespace XmlRpc.Serialization.Exceptions;

public sealed class XmlRpcSerializationException(string message) : Exception(message);
