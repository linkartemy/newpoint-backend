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
    private readonly IImageRepository _imageRepository;
    private readonly IObjectRepository _objectRepository;
    private readonly IFollowRepository _followRepository;

    public UserService(IUserRepository userRepository, IImageRepository imageRepository,
        IObjectRepository objectRepository, IFollowRepository followRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _imageRepository = imageRepository;
        _objectRepository = objectRepository;
        _followRepository = followRepository;
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
                Login = login,
                Name = request.Name,
                Surname = request.Surname,
                Email = email,
                Phone = phone,
                BirthDate = date,
                IP = context.Peer,
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

    public override async Task<Response> ValidateUser(ValidateUserRequest request, ServerCallContext context)
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

            if (await _userRepository.LoginExists(login))
            {
                response.Error = "User with this name already exists";
                response.Status = 400;
                return response;
            }

            response.Data = Any.Pack(new ValidateUserResponse
            {
                Valid = true
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

    public override async Task<Response> UpdateProfile(UpdateProfileRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var name = request.Name.Trim();
            var surname = request.Surname.Trim();
            var description = request.Description.Trim();
            var location = request.Location.Trim();
            var birthDate = request.BirthDate;

            if (description.Length > 255)
            {
                response.Error = "Description's length can't be over 255 symbols";
                response.Status = 400;
                return response;
            }

            if (birthDate == null)
            {
                response.Error = "Birth date can't be null";
                response.Status = 400;
                return response;
            }

            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);

            if (user is null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            user.Name = name;
            user.Surname = surname;
            user.Description = description.Length != 0 ? description : null;
            user.Location = location.Length != 0 ? location : null;
            user.BirthDate = birthDate.ToDateTime();

            await _userRepository.UpdateProfile(user.Id, user);

            response.Data = Any.Pack(new UpdateProfileResponse
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

    public override async Task<Response> UpdateProfileImage(UpdateProfileImageRequest request,
        ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var data = request.Data;
            var name = request.Name.Trim();

            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);

            if (user is null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            await _objectRepository.InsertObject(request.Data.ToByteArray(), name);
            var id = await _imageRepository.InsertImage(name);

            await _userRepository.UpdateProfileImageId(user.Id, id);

            response.Data = Any.Pack(new UpdateProfileImageResponse
            {
                Id = id
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

    public override async Task<Response> ChangeEmail(ChangeEmailRequest request,
        ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var email = request.Email.Trim();

            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);

            if (user is null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            await _userRepository.UpdateEmailById(user.Id, email);

            await SmtpHandler.SendEmail(user.Email, "NewPoint: Email has been changed", "Your email has been changed successfully. If you didn't do this, please contact with us.");

            response.Data = Any.Pack(new ChangeEmailResponse
            {
                Changed = true
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

    public override async Task<Response> ChangePassword(ChangePasswordRequest request,
        ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var currentPassword = request.CurrentPassword.Trim();
            var newPassword = request.NewPassword.Trim();

            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);

            if (user is null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            if (newPassword.Length < 8)
            {
                response.Error = "Password's length must be at least 8 symbols";
                response.Status = 400;
                return response;
            }

            if (newPassword.Length > 32)
            {
                response.Error = "Password's length can't be over 32 symbols";
                response.Status = 400;
                return response;
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");

            if (!hasNumber.IsMatch(newPassword))
            {
                response.Error = "Password must contain at least 1 digit";
                response.Status = 400;
                return response;
            }

            if (!hasUpperChar.IsMatch(newPassword))
            {
                response.Error = "Password must contain at least 1 capital letter";
                response.Status = 400;
                return response;
            }

            if (newPassword.Length > 32)
            {
                response.Error = "Password's length can't be over 32 symbols";
                response.Status = 400;
                return response;
            }

            user.HashedPassword = await _userRepository.GetUserHashedPasswordById(user.Id);

            if (AuthenticationHandler.VerifyPassword(user, currentPassword) is false)
            {
                response.Error = "Wrong current password";
                response.Status = 400;
                return response;
            }

            AuthenticationHandler.AssignPasswordHash(user, newPassword);
            await _userRepository.UpdatePasswordById(user.Id, user.HashedPassword);

            await SmtpHandler.SendEmail(user.Email, "NewPoint: Password has been changed", "Your password has been changed successfully. If you didn't do this, please contact with us.");

            response.Data = Any.Pack(new ChangePasswordResponse
            {
                Changed = true
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

    public override async Task<Response> VerifyPassword(VerifyPasswordRequest request,
        ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var password = request.Password.Trim();

            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);

            if (user is null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            user.HashedPassword = await _userRepository.GetUserHashedPasswordById(user.Id);

            response.Data = Any.Pack(new VerifyPasswordResponse
            {
                Verified = AuthenticationHandler.VerifyPassword(user, password)
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

    public override async Task<Response> Follow(FollowRequest request,
        ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var userId = request.UserId;

            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);

            if (user is null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            if (await _userRepository.CountWithId(userId) is 0)
            {
                response.Error = "User you want to follow doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            var following = true;

            if (!await _followRepository.FollowExists(user.Id, userId))
            {
                await _followRepository.InsertFollow(user.Id, userId);
                await _userRepository.UpdateFollowingByUserId(user.Id,
                    await _userRepository.GetFollowingByUserId(user.Id) + 1);
                await _userRepository.UpdateFollowersByUserId(userId,
                    await _userRepository.GetFollowersByUserId(userId) + 1);
            }
            else
            {
                await _followRepository.DeleteFollow(user.Id, userId);
                await _userRepository.UpdateFollowingByUserId(user.Id,
                    await _userRepository.GetFollowingByUserId(user.Id) - 1);
                await _userRepository.UpdateFollowersByUserId(userId,
                    await _userRepository.GetFollowersByUserId(userId) - 1);
                following = false;
            }

            response.Data = Any.Pack(new FollowResponse
            {
                Following = following
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

    public override async Task<Response> IsFollowing(IsFollowingRequest request,
        ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var userId = request.UserId;

            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);

            if (user is null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }
            if (await _userRepository.CountWithId(userId) is 0)
            {
                response.Error = "User you follow doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            var following = await _followRepository.FollowExists(user.Id, userId);

            response.Data = Any.Pack(new IsFollowingResponse
            {
                Following = following
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