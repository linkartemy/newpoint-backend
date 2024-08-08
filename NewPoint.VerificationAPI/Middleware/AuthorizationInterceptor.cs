using Grpc.Core;
using Grpc.Core.Interceptors;
using NewPoint.VerificationAPI.Clients;

namespace NewPoint.VerificationAPI.Middleware;

public class AuthorizationInterceptor : Interceptor
{
    private readonly IUserClient _userClient;

    public AuthorizationInterceptor(IUserClient userClient)
    {
        _userClient = userClient;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            if (context.RequestHeaders.Any(x => x.Key.ToLower() == "authorization") is false)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Authorization header is missing"));
            }
            var token = context.RequestHeaders.Where(x => x.Key.ToLower() == "authorization").FirstOrDefault()!.Value.Split(' ')[1];
            if (await _userClient.UserExistsByToken(token) is false)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token"));
            }
            return await continuation(request, context);
        }
        catch (Exception e)
        {
            throw;
        }
    }
}