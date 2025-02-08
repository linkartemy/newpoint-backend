using Grpc.Core;
using Grpc.Core.Interceptors;
using NewPoint.UserAPI.Repositories;
using NewPoint.UserAPI.Services;
using static NewPoint.UserAPI.Services.UserService;

namespace NewPoint.UserAPI.Middleware;

public class AuthorizationInterceptor : Interceptor
{
    public static class AuthorizationInterceptorErrorMessages
    {
        public const string AuthorizationHeaderMissing = "Authorization header is missing";
    }

    public static class AuthorizationInterceptorErrorCodes
    {
        public const string AuthorizationHeaderMissing = "authorization_header_missing";
    }

    private readonly IUserRepository _userRepository;

    public AuthorizationInterceptor(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            if (context.Method.Contains(nameof(UserService.Login)) is true
            || context.Method.Contains(nameof(UserService.Register)) is true
            || context.Method.Contains(nameof(UserService.ValidateUser)) is true)
            {
                return await continuation(request, context);
            }
            if (context.RequestHeaders.Any(x => x.Key.ToLower() == "authorization") is false)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, AuthorizationInterceptorErrorCodes.AuthorizationHeaderMissing),
                message: AuthorizationInterceptorErrorMessages.AuthorizationHeaderMissing);
            }
            var token = context.RequestHeaders.Where(x => x.Key.ToLower() == "authorization").FirstOrDefault()!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);
            if (user is null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, UserServiceErrorCodes.UserDoesntExist),
                message: UserServiceErrorMessages.UserDoesntExist);
            }
            context.UserState.Add("user", user);
            context.UserState.Add("token", token);
            return await continuation(request, context);
        }
        catch (Exception e)
        {
            throw;
        }
    }
}