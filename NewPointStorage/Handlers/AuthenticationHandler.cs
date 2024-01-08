using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NewPointStorage.Handlers;

public static class AuthenticationHandler
{
    public static string JwtToken { get; set; } = string.Empty;

    public static bool IsTokenExpired(string token)
    {
        return DateTime.Now >= new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
    }

    public static int GetIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        return int.Parse(jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.UserData).Value);
    }
}