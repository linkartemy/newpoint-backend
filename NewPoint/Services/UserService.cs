using System.Text.RegularExpressions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Extensions;
using NewPoint.Handlers;
using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class UserService : GrpcUser.GrpcUserBase
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public override async Task<Response> Login(LoginRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var login = request.Login.Trim();
            var password = request.Password.Trim();

            if (await _userRepository.LoginExists(login) is false)
            {
                response.Error = "Wrong login or password";
                response.Status = 400;
                return response;
            }

            var user = await _userRepository.GetUserByLogin(login);
            user.HashedPassword = await _userRepository.GetUserHashedPassword(user.Login);

            if (AuthenticationHandler.VerifyPassword(user, password) is false)
            {
                response.Error = "Wrong login or password";
                response.Status = 400;
                return response;
            }

            var token = await _userRepository.GetTokenById(user.Id);

            if (AuthenticationHandler.IsTokenExpired(token))
            {
                token = AuthenticationHandler.CreateToken(user);
                await _userRepository.UpdateToken(user.Id, token);
            }

            context.GetHttpContext().Response.Headers.Add("Authorization", token);

            response.Data = Any.Pack(user.ToUserModel());
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> Register(RegisterRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var login = request.Login.Trim();

            if (login.Length < 4)
            {
                response.Error = "Username must be at least 4 symbols";
                response.Status = 400;
                return response;
            }

            if (login.Length > 32)
            {
                response.Error = "Username's length can't be over 32 symbols";
                response.Status = 400;
                return response;
            }

            var password = request.Password.Trim();

            if (password.Length < 8)
            {
                response.Error = "Password's length must be at least 8 symbols";
                response.Status = 400;
                return response;
            }

            if (password.Length > 32)
            {
                response.Error = "Password's length can't be over 32 symbols";
                response.Status = 400;
                return response;
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");

            if (!hasNumber.IsMatch(password))
            {
                response.Error = "Password must contain at least 1 digit";
                response.Status = 400;
                return response;
            }

            if (!hasUpperChar.IsMatch(password))
            {
                response.Error = "Password must contain at least 1 capital letter";
                response.Status = 400;
                return response;
            }

            if (password.Length > 32)
            {
                response.Error = "Password's length can't be over 32 symbols";
                response.Status = 400;
                return response;
            }

            var email = request.Email.Trim();
            var phone = request.Phone.Trim();

            if (email.Length == 0 && phone.Length == 0)
            {
                response.Error = "You must provide either email or phone number";
                response.Status = 400;
                return response;
            }

            if (DateTimeHandler.TryTimestampToDateTime(request.BirthDate, out var date) is false) date = DateTime.Today;

            var user = new User
            {
                Login = login, Name = request.Name, Surname = request.Surname, Email = email, Phone = phone,
                BirthDate = date, IP = context.Peer,
                LastLoginTimestamp = DateTime.Now
            };

            if (await _userRepository.LoginExists(login))
            {
                response.Error = "User with this name already exists";
                response.Status = 400;
                return response;
            }

            AuthenticationHandler.AssignPasswordHash(user, password);

            var token = AuthenticationHandler.CreateToken(user);

            await _userRepository.InsertUser(user, token);

            context.GetHttpContext().Response.Headers.Add("Authorization", token);

            response.Data = Any.Pack(new RegisterResponse
            {
                User = user.ToUserModel()
            });

            return response;
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            response.Status = 500;
            return response;
        }
    }

    public override async Task<Response> GetUserByToken(GetUserByTokenRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);

            if (user is null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            response.Data = Any.Pack(new GetUserByTokenResponse
            {
                User = user.ToUserModel()
            });

            return response;
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            response.Status = 500;
            return response;
        }
    }
    
    public override async Task<Response> GetProfileById(GetProfileByIdRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = await _userRepository.GetProfileById(request.Id);

            if (user is null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            response.Data = Any.Pack(new GetProfileByIdResponse
            {
                User = user.ToUserModel()
            });

            return response;
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            response.Status = 500;
            return response;
        }
    }
}