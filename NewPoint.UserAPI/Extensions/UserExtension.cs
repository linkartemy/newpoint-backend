using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.Common.Extensions;

namespace NewPoint.UserAPI.Extensions;

public static class UserExtension
{
    public static UserModel ToUserModel(this User user)
    {
        return new UserModel
        {
            Id = user.Id,
            Login = user.Login,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            Description = user.Description,
            Location = user.Location,
            ProfileImageId = user.ProfileImageId,
            HeaderImageId = user.HeaderImageId,
            Ip = user.IP,
            Phone = user.Phone,
            LastLoginTimestamp = DateTimeHandler.DateTimeToTimestamp(user.LastLoginTimestamp),
            RegistrationTimestamp = DateTimeHandler.DateTimeToTimestamp(user.RegistrationTimestamp),
            BirthDate = DateTimeHandler.DateToTimestamp(user.BirthDate),
            Followers = user.Followers,
            Following = user.Following
        };
    }
}