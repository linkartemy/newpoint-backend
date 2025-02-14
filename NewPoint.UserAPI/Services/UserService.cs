using System.Text.RegularExpressions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.StaticFiles;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.UserAPI.Extensions;
using NewPoint.UserAPI.Repositories;

namespace NewPoint.UserAPI.Services;

public class UserService : GrpcUser.GrpcUserBase
{
    public static class UserServiceErrorMessages
    {
        public const string GenericError = "Something went wrong. Please try again later. We are sorry";
        public const string WrongLoginOrPassword = "Wrong login or password";
        public const string UsernameMustBeAtLeastFourSymbols = "Username must be at least 4 symbols";
        public const string UsernameLengthCantBeOverThirtyTwoSymbols = "Username's length can't be over 32 symbols";
        public const string PasswordLengthMustBeAtLeastEightSymbols = "Password's length must be at least 8 symbols";
        public const string PasswordLengthCantBeOverThirtyTwoSymbols = "Password's length can't be over 32 symbols";
        public const string PasswordMustContainAtLeastOneDigit = "Password must contain at least 1 digit";
        public const string PasswordMustContainAtLeastOneCapitalLetter = "Password must contain at least 1 capital letter";
        public const string EmailOrPhoneMustBeProvided = "You must provide at least email or phone number";
        public const string UserWithThisNameAlreadyExists = "User with this name already exists";
        public const string UserWithThisEmailAlreadyExists = "User with this email already exists";
        public const string UserDoesntExist = "User doesn't exist. Server error. Please contact with us";
    }

    public static class UserServiceErrorCodes
    {
        public const string GenericError = "generic_error";
        public const string WrongLoginOrPassword = "wrong_login_or_password";
        public const string UsernameMustBeAtLeastFourSymbols = "username_must_be_at_least_four_symbols";
        public const string UsernameLengthCantBeOverThirtyTwoSymbols = "username_length_cant_be_over_thirty_two_symbols";
        public const string PasswordLengthMustBeAtLeastEightSymbols = "password_length_must_be_at_least_eight_symbols";
        public const string PasswordLengthCantBeOverThirtyTwoSymbols = "password_length_cant_be_over_thirty_two_symbols";
        public const string PasswordMustContainAtLeastOneDigit = "password_must_contain_at_least_one_digit";
        public const string PasswordMustContainAtLeastOneCapitalLetter = "password_must_contain_at_least_one_capital_letter";
        public const string EmailOrPhoneMustBeProvided = "email_or_phone_must_be_provided";
        public const string UserWithThisNameAlreadyExists = "user_with_this_name_already_exists";
        public const string UserWithThisEmailAlreadyExists = "user_with_this_email_already_exists";
        public const string UserDoesntExist = "user_doesnt_exist";
    }

    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IFollowRepository _followRepository;

    public UserService(IUserRepository userRepository, IFollowRepository followRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _followRepository = followRepository;
        _logger = logger;
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var login = request.Login.Trim();
        var password = request.Password.Trim();

        int usersWithLogin;
        int usersWithEmail;
        try
        {
            usersWithLogin = await _userRepository.CountByLogin(login);
            usersWithEmail = await _userRepository.CountByEmail(login);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while counting users by login or email");
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }

        if (usersWithLogin is 0 && usersWithEmail is 0)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.WrongLoginOrPassword,
            UserServiceErrorMessages.WrongLoginOrPassword);
        }

        User user;
        try
        {
            if (usersWithLogin is not 0)
            {
                user = await _userRepository.GetUserByLogin(login);
            }
            else
            {
                user = await _userRepository.GetUserByEmail(login);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting user by login or email");
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
        user.HashedPassword = await _userRepository.GetUserHashedPassword(user.Login);

        if (AuthenticationHandler.VerifyPassword(user, password) is false)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.WrongLoginOrPassword,
            UserServiceErrorMessages.WrongLoginOrPassword);
        }

        string token;
        try
        {
            token = await _userRepository.GetTokenById(user.Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting token by id");
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }

        if (AuthenticationHandler.IsTokenExpired(token))
        {
            token = AuthenticationHandler.CreateToken(user);
            await _userRepository.UpdateToken(user.Id, token);
        }

        context.GetHttpContext().Response.Headers.Append("Authorization", token);

        return new LoginResponse
        {
            User = user.ToUserModel()
        };
    }

    public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        var login = request.Login.Trim();

        if (login.Length < 4)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UsernameMustBeAtLeastFourSymbols,
            UserServiceErrorMessages.UsernameMustBeAtLeastFourSymbols);
        }

        if (login.Length > 32)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UsernameLengthCantBeOverThirtyTwoSymbols,
            UserServiceErrorMessages.UsernameLengthCantBeOverThirtyTwoSymbols);
        }

        var password = request.Password.Trim();

        if (password.Length < 8)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordLengthMustBeAtLeastEightSymbols,
            UserServiceErrorMessages.PasswordLengthMustBeAtLeastEightSymbols);
        }

        if (password.Length > 32)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordLengthCantBeOverThirtyTwoSymbols,
            UserServiceErrorMessages.PasswordLengthCantBeOverThirtyTwoSymbols);
        }

        var hasNumber = new Regex(@"[0-9]+");
        var hasUpperChar = new Regex(@"[A-Z]+");

        if (!hasNumber.IsMatch(password))
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordMustContainAtLeastOneDigit,
            UserServiceErrorMessages.PasswordMustContainAtLeastOneDigit);
        }

        if (!hasUpperChar.IsMatch(password))
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordMustContainAtLeastOneCapitalLetter,
            UserServiceErrorMessages.PasswordMustContainAtLeastOneCapitalLetter);
        }

        if (password.Length > 32)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordLengthCantBeOverThirtyTwoSymbols,
            UserServiceErrorMessages.PasswordLengthCantBeOverThirtyTwoSymbols);
        }

        var email = request.Email.Trim();
        var phone = request.Phone.Trim();

        if (email.Length == 0 && phone.Length == 0)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.EmailOrPhoneMustBeProvided,
            UserServiceErrorMessages.EmailOrPhoneMustBeProvided);
        }

        if (DateTimeHandler.TryTimestampToDateTime(request.BirthDate, out var date) is false) date = DateTime.Today;

        // var profileImageId = await _imageRepository.GetImageIdByName("0.png");

        var user = new User
        {
            Login = login,
            Name = request.Name,
            Surname = request.Surname,
            Email = email,
            Phone = phone,
            BirthDate = date,
            ProfileImageId = 0,
            IP = context.Peer,
            LastLoginTimestamp = DateTime.Now
        };

        if (await _userRepository.CountByLogin(login) != 0)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UserWithThisNameAlreadyExists,
            UserServiceErrorMessages.UserWithThisNameAlreadyExists);
        }

        if (await _userRepository.CountByEmail(email) != 0)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UserWithThisEmailAlreadyExists,
            UserServiceErrorMessages.UserWithThisEmailAlreadyExists);
        }

        AuthenticationHandler.AssignPasswordHash(user, password);

        var token = AuthenticationHandler.CreateToken(user);

        await _userRepository.InsertUser(user, token);

        context.GetHttpContext().Response.Headers.Append("Authorization", token);

        return new RegisterResponse
        {
            User = user.ToUserModel()
        };
    }

    public override async Task<GetUserByTokenResponse> GetUserByToken(GetUserByTokenRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.RetrieveUser();
            return new GetUserByTokenResponse
            {
                User = user.ToUserModel()
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
    }

    public override async Task<GetProfileByIdResponse> GetProfileById(GetProfileByIdRequest request, ServerCallContext context)
    {
        User? user;
        try
        {
            user = await _userRepository.GetProfileById(request.Id);
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }

        if (user is null)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UserDoesntExist,
            UserServiceErrorMessages.UserDoesntExist);
        }

        return new GetProfileByIdResponse
        {
            User = user.ToUserModel()
        };
    }

    public override async Task<ValidateUserResponse> ValidateUser(ValidateUserRequest request, ServerCallContext context)
    {
        var login = request.Login.Trim().ToLower();

        if (login.Length < 4)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UsernameMustBeAtLeastFourSymbols,
            UserServiceErrorMessages.UsernameMustBeAtLeastFourSymbols);
        }

        if (login.Length > 32)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UsernameLengthCantBeOverThirtyTwoSymbols,
            UserServiceErrorMessages.UsernameLengthCantBeOverThirtyTwoSymbols);
        }

        var password = request.Password.Trim();

        if (password.Length < 8)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordLengthMustBeAtLeastEightSymbols,
            UserServiceErrorMessages.PasswordLengthMustBeAtLeastEightSymbols);
        }

        if (password.Length > 32)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordLengthCantBeOverThirtyTwoSymbols,
            UserServiceErrorMessages.PasswordLengthCantBeOverThirtyTwoSymbols);
        }

        var hasNumber = new Regex(@"[0-9]+");
        var hasUpperChar = new Regex(@"[A-Z]+");

        if (!hasNumber.IsMatch(password))
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordMustContainAtLeastOneDigit,
            UserServiceErrorMessages.PasswordMustContainAtLeastOneDigit);
        }

        if (!hasUpperChar.IsMatch(password))
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordMustContainAtLeastOneCapitalLetter,
            UserServiceErrorMessages.PasswordMustContainAtLeastOneCapitalLetter);
        }

        if (password.Length > 32)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordLengthCantBeOverThirtyTwoSymbols,
            UserServiceErrorMessages.PasswordLengthCantBeOverThirtyTwoSymbols);
        }

        var email = request.Email.Trim();
        var phone = request.Phone.Trim();

        if (email.Length == 0 && phone.Length == 0)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.EmailOrPhoneMustBeProvided,
            UserServiceErrorMessages.EmailOrPhoneMustBeProvided);
        }

        if (await _userRepository.CountByLogin(login) != 0)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UserWithThisNameAlreadyExists,
            UserServiceErrorMessages.UserWithThisNameAlreadyExists);
        }

        if (await _userRepository.CountByEmail(email) != 0)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UserWithThisEmailAlreadyExists,
            UserServiceErrorMessages.UserWithThisEmailAlreadyExists);
        }

        return new ValidateUserResponse
        {
            Valid = true
        };
    }

    public override async Task<UpdateProfileResponse> UpdateProfile(UpdateProfileRequest request, ServerCallContext context)
    {
        var name = request.Name.Trim();
        var surname = request.Surname.Trim();
        var description = request.Description.Trim();
        var location = request.Location.Trim();
        var birthDate = request.BirthDate;

        if (description.Length > 255)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UsernameLengthCantBeOverThirtyTwoSymbols,
            UserServiceErrorMessages.UsernameLengthCantBeOverThirtyTwoSymbols);
        }

        if (birthDate == null)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UsernameLengthCantBeOverThirtyTwoSymbols,
            UserServiceErrorMessages.UsernameLengthCantBeOverThirtyTwoSymbols);
        }

        var user = context.RetrieveUser();

        if (user is null)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UserDoesntExist,
            UserServiceErrorMessages.UserDoesntExist);
        }

        user.Name = name;
        user.Surname = surname;
        user.Description = description.Length != 0 ? description : null;
        user.Location = location.Length != 0 ? location : null;
        user.BirthDate = birthDate.ToDateTime();

        await _userRepository.UpdateProfile(user.Id, user);

        return new UpdateProfileResponse
        {
            User = user.ToUserModel()
        };
    }

    // public override async Task<Response> UpdateProfileImage(UpdateProfileImageRequest request,
    //     ServerCallContext context)
    // {
    //     var response = new Response
    //     {
    //         Status = 200
    //     };
    //     try
    //     {
    //         var data = request.Data;
    //         var name = request.Name.Trim();

    //         var user = context.RetrieveUser();

    //         if (user is null)
    //         {
    //             response.Error = "User doesn't exist. Server error. Please contact with us";
    //             response.Status = 400;
    //             return response;
    //         }

    //         var fileDetails = name.Split('.');
    //         var extension = "jpeg";
    //         if (fileDetails.Length > 1)
    //         {
    //             extension = fileDetails.Last();
    //         }
    //         while (await _imageRepository.Count(name) != 0)
    //         {
    //             name = StringHandler.GenerateString(32);
    //             name += "." + extension;
    //         }
    //         var contentType = "image/jpeg";
    //         new FileExtensionContentTypeProvider().TryGetContentType(name, out contentType);
    //         await _objectRepository.InsertObject(data.ToByteArray(), S3Handler.Configuration.UserImagesBucket, name, contentType);
    //         var id = await _imageRepository.InsertImage(name);

    //         await _userRepository.UpdateProfileImageId(user.Id, id);

    //         response.Data = Any.Pack(new UpdateProfileImageResponse
    //         {
    //             Id = id
    //         });

    //         return response;
    //     }
    //     catch (Exception)
    //     {
    //         response.Error = "Something went wrong. Please try again later. We are sorry";
    //         response.Status = 500;
    //         return response;
    //     }
    // }

    public override async Task<ChangeEmailResponse> ChangeEmail(ChangeEmailRequest request,
        ServerCallContext context)
    {
        var email = request.Email.Trim();
        var user = context.RetrieveUser();

        if (user is null)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UserDoesntExist,
            UserServiceErrorMessages.UserDoesntExist);
        }
        if (await _userRepository.CountByEmail(email) != 0)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UserWithThisEmailAlreadyExists,
            UserServiceErrorMessages.UserWithThisEmailAlreadyExists);
        }

        try
        {
            await _userRepository.UpdateEmailById(user.Id, email);
            await SmtpHandler.SendEmail(user.Email, "NewPoint: Email has been changed", "Your email has been changed successfully. If you didn't do this, please contact with us.");
            return new ChangeEmailResponse
            {
                Changed = true
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
    }

    public override async Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request,
        ServerCallContext context)
    {

        var currentPassword = request.CurrentPassword.Trim();
        var newPassword = request.NewPassword.Trim();

        var user = context.RetrieveUser();

        if (newPassword.Length < 8)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordLengthMustBeAtLeastEightSymbols,
            UserServiceErrorMessages.PasswordLengthMustBeAtLeastEightSymbols);
        }

        if (newPassword.Length > 32)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordLengthCantBeOverThirtyTwoSymbols,
            UserServiceErrorMessages.PasswordLengthCantBeOverThirtyTwoSymbols);
        }

        var hasNumber = new Regex(@"[0-9]+");
        var hasUpperChar = new Regex(@"[A-Z]+");

        if (!hasNumber.IsMatch(newPassword))
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordMustContainAtLeastOneDigit,
            UserServiceErrorMessages.PasswordMustContainAtLeastOneDigit);
        }

        if (!hasUpperChar.IsMatch(newPassword))
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordMustContainAtLeastOneCapitalLetter,
            UserServiceErrorMessages.PasswordMustContainAtLeastOneCapitalLetter);
        }

        if (newPassword.Length > 32)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.PasswordLengthCantBeOverThirtyTwoSymbols,
            UserServiceErrorMessages.PasswordLengthCantBeOverThirtyTwoSymbols);
        }

        user.HashedPassword = await _userRepository.GetUserHashedPasswordById(user.Id);

        if (AuthenticationHandler.VerifyPassword(user, currentPassword) is false)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.WrongLoginOrPassword,
            UserServiceErrorMessages.WrongLoginOrPassword);
        }
        try
        {
            AuthenticationHandler.AssignPasswordHash(user, newPassword);
            await _userRepository.UpdatePasswordById(user.Id, user.HashedPassword);

            await SmtpHandler.SendEmail(user.Email, "NewPoint: Password has been changed", "Your password has been changed successfully. If you didn't do this, please contact with us.");

            return new ChangePasswordResponse
            {
                Changed = true
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
    }

    public override async Task<VerifyPasswordResponse> VerifyPassword(VerifyPasswordRequest request,
        ServerCallContext context)
    {
        try
        {
            var password = request.Password.Trim();
            var user = context.RetrieveUser();
            user.HashedPassword = await _userRepository.GetUserHashedPasswordById(user.Id);
            return new VerifyPasswordResponse
            {
                Verified = AuthenticationHandler.VerifyPassword(user, password)
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
    }

    public override async Task<FollowResponse> Follow(FollowRequest request,
        ServerCallContext context)
    {
        var userId = request.UserId;
        var user = context.RetrieveUser();
        if (await _userRepository.CountWithId(userId) is 0)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UserDoesntExist,
            UserServiceErrorMessages.UserDoesntExist);
        }
        try
        {
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

            return new FollowResponse
            {
                Following = following
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
    }

    public override async Task<IsFollowingResponse> IsFollowing(IsFollowingRequest request,
        ServerCallContext context)
    {
        var userId = request.UserId;
        var user = context.RetrieveUser();
        if (await _userRepository.CountWithId(userId) is 0)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.InvalidArgument, UserServiceErrorCodes.UserDoesntExist,
            UserServiceErrorMessages.UserDoesntExist);
        }

        try
        {
            var following = await _followRepository.FollowExists(user.Id, userId);
            return new IsFollowingResponse
            {
                Following = following
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
    }

    public override async Task<GetTwoFactorByTokenResponse> GetTwoFactorByToken(GetTwoFactorByTokenRequest request, ServerCallContext context)
    {
        try
        {
            var token = request.Token;
            var user = await _userRepository.GetUserByToken(token);

            var twoFactor = await _userRepository.GetTwoFactorById(user.Id);

            return new GetTwoFactorByTokenResponse
            {
                Enabled = twoFactor
            };
        }
        catch (Exception e)
        {
            _logger.LogError("Error while getting two factor by token: " + e.Message);
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
    }

    public override async Task<UpdateTwoFactorResponse> UpdateTwoFactor(UpdateTwoFactorRequest request, ServerCallContext context)
    {
        try
        {
            var enabled = request.Enabled;
            var user = context.RetrieveUser();
            await _userRepository.UpdateTwoFactorById(user.Id, enabled);
            return new UpdateTwoFactorResponse
            {
                Updated = enabled
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
    }

    public override async Task<GetUserByLoginResponse> GetUserByLogin(GetUserByLoginRequest request, ServerCallContext context)
    {
        try
        {
            var login = request.Login.Trim();
            var user = await _userRepository.GetUserByLogin(login);
            return new GetUserByLoginResponse
            {
                User = user.ToUserModel()
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
    }

    public override async Task<GetPostUserDataByIdResponse> GetPostUserDataById(GetPostUserDataByIdRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _userRepository.GetPostUserDataById(request.Id);
            return new GetPostUserDataByIdResponse
            {
                User = user.ToUserModel()
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(StatusCode.Internal, UserServiceErrorCodes.GenericError,
            UserServiceErrorMessages.GenericError);
        }
    }
}