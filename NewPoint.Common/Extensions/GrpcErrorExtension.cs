using NewPoint.Common.Models;
using Grpc.Core;
using Status = Google.Rpc.Status;

namespace NewPoint.Common.Extensions;

public static class GrpcErrorExtension
{
    public static RpcException ToRpcException(this GrpcError data)
    {
        return new Status
        {
            Code = (int)data.Code,
            Message = data.Message,
            Details = { data.Details }
        }.ToRpcException();
    }
}