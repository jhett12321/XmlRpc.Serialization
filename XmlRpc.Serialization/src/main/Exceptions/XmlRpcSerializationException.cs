using System;

namespace XmlRpc.Serialization.Exceptions;

public class XmlRpcSerializationException(string message) : Exception(message);
