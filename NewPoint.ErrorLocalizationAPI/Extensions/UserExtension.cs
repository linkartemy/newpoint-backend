using Google.Protobuf.WellKnownTypes;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;

namespace NewPoint.ErrorLocalizationAPI.Extensions;

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
            Email = user.HasEmail ? user.Email : null,
            Description = user.HasDescription ? user.Description : null,
            Location = user.HasLocation ? user.Location : null,
            ProfileImageId = user.ProfileImageId,
            HeaderImageId = user.HeaderImageId,
            IP = user.HasIp ? user.Ip : null,
            Phone = user.HasPhone ? user.Phone : null,
            LastLoginTimestamp = DateTimeHandler.TimestampToDateTime(
                user.LastLoginTimestampWrapperCase == UserModel.LastLoginTimestampWrapperOneofCase.LastLoginTimestamp
                ? user.LastLoginTimestamp : new Timestamp()) ?? DateTime.MinValue,
            RegistrationTimestamp = DateTimeHandler.TimestampToDateTime(
                user.RegistrationTimestampWrapperCase == UserModel.RegistrationTimestampWrapperOneofCase.RegistrationTimestamp
                ? user.RegistrationTimestamp : new Timestamp()) ?? DateTime.MinValue,
            BirthDate = DateTimeHandler.TimestampToDateTime(
                user.BirthDateWrapperCase == UserModel.BirthDateWrapperOneofCase.BirthDate
                ? user.BirthDate : new Timestamp()) ?? DateTime.MinValue,
            Followers = user.Followers,
            Following = user.Following
        };
    }
}