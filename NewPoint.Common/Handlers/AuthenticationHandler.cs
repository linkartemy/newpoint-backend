using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NewPoint.Common.Models;

namespace NewPoint.Common.Handlers;

public static class AuthenticationHandler
{
    public static string JwtToken { get; set; } = string.Empty;

    public static void AssignPasswordHash(User user, string password)
    {
        var passwordHasher = new PasswordHasher<User>();
        user.HashedPassword = passwordHasher.HashPassword(user, password);
    }

    public static bool VerifyPassword(User user, string password)
    {
        var passwordHasher = new PasswordHasher<User>();
        return passwordHasher.VerifyHashedPassword(user, user.HashedPassword, password) is PasswordVerificationResult
            .Success;
    }

    public static string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.UserData, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtToken));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(90),
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler().WriteToken(token);
        return handler;
    }

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