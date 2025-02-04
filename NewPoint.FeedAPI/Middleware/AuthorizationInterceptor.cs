using Grpc.Core;
using Grpc.Core.Interceptors;
using NewPoint.FeedAPI.Clients;

namespace NewPoint.FeedAPI.Middleware;

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
            var user = await _userClient.GetUserByToken(token);
            if (await _userClient.UserExistsByToken(token) is false)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token"));
            }
            context.UserState.Add("token", token);
            context.UserState.Add("user", user);
            return await continuation(request, context);
        }
        catch (Exception e)
        {
            throw;
        }
    }
}