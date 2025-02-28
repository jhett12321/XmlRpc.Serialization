using XmlRpc.Serialization.Converters;

namespace XmlRpc.Serialization.Tests.Models;

public sealed class DeserializeRequestMethod<TParam1, TParam2>(DeserializeRequestMethod<TParam1, TParam2>.RequestHandler handler) : IXmlRpcRequestMethod<DeserializeRequestMethod<TParam1, TParam2>.RequestHandler>
{
  public delegate void RequestHandler(TParam1 param1, TParam2 param2);

  public RequestHandler Handler { get; } = handler;

  public void HandleRequestMessage(XmlRpcReader? reader)
  {
    ArgumentNullException.ThrowIfNull(reader, nameof(reader));

    TParam1 param1 = reader.ReadParameter(paramReader => XmlRpcConverterFactory.GetBuiltInValueConverter<TParam1>().Deserialize(paramReader));
    TParam2 param2 = reader.ReadParameter(paramReader => XmlRpcConverterFactory.GetBuiltInValueConverter<TParam2>().Deserialize(paramReader));
    Handler(param1, param2);
  }
}

public sealed class DeserializeRequestMethod<TParam1>(DeserializeRequestMethod<TParam1>.RequestHandler handler) : IXmlRpcRequestMethod<DeserializeRequestMethod<TParam1>.RequestHandler>
{
  public delegate void RequestHandler(TParam1 param1);

  public RequestHandler Handler { get; } = handler;

  public void HandleRequestMessage(XmlRpcReader? reader)
  {
    ArgumentNullException.ThrowIfNull(reader, nameof(reader));

    TParam1 param1 = reader.ReadParameter(paramReader => XmlRpcConverterFactory.GetBuiltInValueConverter<TParam1>().Deserialize(paramReader));
    Handler(param1);
  }
}

public sealed class DeserializeRequestMethod(DeserializeRequestMethod.RequestHandler handler) : IXmlRpcRequestMethod
{
  public delegate void RequestHandler();

  public RequestHandler Handler { get; } = handler;

  public void HandleRequestMessage(XmlRpcReader? reader)
  {
    Handler();
  }
}
