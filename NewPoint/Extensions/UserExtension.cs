using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Extensions;

public static class UserExtension
{
    public static UserModel ToUserModel(this User user)
    {
        return new UserModel
        {
            Id = user.Id, Login = user.Login, Name = user.Name, Surname = user.Surname,
            Email = user.Email.ToNullableString(),
            Description = user.Description.ToNullableString(),
            Location = user.Location.ToNullableString(),
            ProfileImage = user.ProfileImage.ToNullableString(),
            HeaderImage = user.HeaderImage.ToNullableString(), Ip = user.IP.ToNullableString(), Phone = user.Phone.ToNullableString(),
            LastLoginTimestamp = DateTimeHandler.DateTimeToTimestamp(user.LastLoginTimestamp).ToNullableTimestamp(),
            RegistrationTimestamp = DateTimeHandler.DateTimeToTimestamp(user.RegistrationTimestamp).ToNullableTimestamp(),
            BirthDate = DateTimeHandler.DateTimeToTimestamp(user.BirthDate.AddHours(10)).ToNullableTimestamp()
        };
    }
}