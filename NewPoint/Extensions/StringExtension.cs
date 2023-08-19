using NewPoint.Models;

namespace NewPoint.Handlers;

public static class UserExtension
{
    public static UserModel ToUserModel(this User user)
    {
        return new UserModel
        {
            Id = user.Id, Login = user.Login, Email = user.Email.ToNullableString(),
            ProfileImage = user.ProfileImage.ToNullableString(),
            HeaderImage = user.HeaderImage.ToNullableString(), Ip = user.IP, Phone = user.Phone.ToNullableString(),
            LastLoginTimestamp = DateTimeHandler.TimestampToDateTime(user.LastLoginTimestamp),
            RegistrationTimestamp = DateTimeHandler.TimestampToDateTime(user.RegistrationTimestamp),
            BirthDate = DateTimeHandler.TimestampToDateTime(user.BirthDate)
        };
    }
}

public static class StringExtension
{
    public static NullableString ToNullableString(this string? data)
    {
        return data is null
            ? new NullableString { Null = new Google.Protobuf.WellKnownTypes.NullValue() }
            : new NullableString { Data = data };
    }
}