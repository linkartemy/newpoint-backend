
using Grpc.Core;

namespace NewPoint.Common.Handlers;

public static class ExceptionHandler
{
    public static RpcException CreateRpcException(StatusCode statusCode, string errorCode, string message)
    {
        return new RpcException(new Status(statusCode, errorCode),
        trailers: new Metadata
        {
            { "fallback_message", message }
        });
    }
}
