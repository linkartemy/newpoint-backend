using Grpc.Core;
using Grpc.Core.Interceptors;
using NewPoint.BookmarkAPI.Clients;

namespace NewPoint.BookmarkAPI.Middleware;

public class AuthorizationInterceptor : Interceptor
{
    public static class AuthorizationInterceptorErrorMessages
    {
        public const string AuthorizationHeaderMissing = "Authorization header is missing";
        public const string UserDoesntExist = "User doesn't exist";
    }

    public static class AuthorizationInterceptorErrorCodes
    {
        public const string AuthorizationHeaderMissing = "authorization_header_missing";
        public const string UserDoesntExist = "user_doesnt_exist";
    }

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
                throw new RpcException(new Status(StatusCode.Unauthenticated, AuthorizationInterceptorErrorCodes.AuthorizationHeaderMissing),
                message: AuthorizationInterceptorErrorMessages.AuthorizationHeaderMissing);
            }
            var token = context.RequestHeaders.Where(x => x.Key.ToLower() == "authorization").FirstOrDefault()!.Value.Split(' ')[1];
            var user = await _userClient.GetUserByToken(token);
            if (await _userClient.UserExistsByToken(token) is false)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, AuthorizationInterceptorErrorCodes.UserDoesntExist),
                message: AuthorizationInterceptorErrorMessages.UserDoesntExist);
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