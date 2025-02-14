using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.Common.Extensions;

namespace NewPoint.UserAPI.Extensions;

public static class UserExtension
{
    public static UserModel ToUserModel(this User user)
    {
        var userModel = new UserModel
        {
            Id = user.Id,
            Login = user.Login,
            Name = user.Name,
            Surname = user.Surname,
            ProfileImageId = user.ProfileImageId,
            HeaderImageId = user.HeaderImageId,
            LastLoginTimestamp = DateTimeHandler.DateTimeToTimestamp(user.LastLoginTimestamp),
            RegistrationTimestamp = DateTimeHandler.DateTimeToTimestamp(user.RegistrationTimestamp),
            BirthDate = DateTimeHandler.DateToTimestamp(user.BirthDate),
            Followers = user.Followers,
            Following = user.Following
        };
        if (userModel.HasEmail)
        {
            userModel.Email = user.Email;
        }
        if (userModel.HasPhone)
        {
            userModel.Phone = user.Phone;
        }
        if (userModel.HasDescription)
        {
            userModel.Description = user.Description;
        }
        if (userModel.HasLocation)
        {
            userModel.Location = user.Location;
        }
        if (userModel.HasIp)
        {
            userModel.Ip = user.IP;
        }
        return userModel;
    }
}