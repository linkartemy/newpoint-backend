using Grpc.Core;
using Grpc.Core.Interceptors;
using NewPoint.Repositories;
using NewPoint.Services;

public class AuthorizationInterceptor : Interceptor
{
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
            if (context.Method.Contains($"NewPoint.GrpcUser{nameof(UserService.Login)}") is false || context.Method.Contains($"NewPoint.GrpcUser{nameof(UserService.Register)}") is false)
            {
                return await continuation(request, context);
            }
            if (context.RequestHeaders.Any(x => x.Key == "Authorization") is false)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Authorization header is missing"));
            }
            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);
            if (user is null)
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