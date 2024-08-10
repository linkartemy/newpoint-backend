using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.Common.Extensions;
using Google.Protobuf.WellKnownTypes;

namespace NewPoint.ObjectAPI.Extensions;

public static class UserExtension
{
    public static User ToUser(this UserModel user)
    {
        return new User
        {
            Id = user.Id,
            Login = user.Login,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email.FromNullableString(),
            Description = user.Description.FromNullableString(),
            Location = user.Location.FromNullableString(),
            ProfileImageId = user.ProfileImageId,
            HeaderImageId = user.HeaderImageId,
            IP = user.Ip.FromNullableString(),
            Phone = user.Phone.FromNullableString(),
            LastLoginTimestamp = DateTimeHandler.TimestampToDateTime(user.LastLoginTimestamp.FromNullableTimestamp() ?? new Timestamp()) ?? DateTime.MinValue,
            RegistrationTimestamp = DateTimeHandler.TimestampToDateTime(user.RegistrationTimestamp.FromNullableTimestamp() ?? new Timestamp()) ?? DateTime.MinValue,
            BirthDate = DateTimeHandler.TimestampToDateTime(user.BirthDate.FromNullableTimestamp() ?? new Timestamp()) ?? DateTime.MinValue,
            Followers = user.Followers,
            Following = user.Following
        };
    }
}