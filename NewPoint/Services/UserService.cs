using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NewPoint.Configurations;
using NewPoint.Models;
using NewPoint.Models.Requests;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class UserService : IUserService
{
    private IOptions<JwtConfiguration> _jwtConfiguration;
    private IUserRepository _userRepository;

    public UserService(IOptions<JwtConfiguration> jwtConfiguration, IUserRepository userRepository)
    {
        _jwtConfiguration = jwtConfiguration;
        _userRepository = userRepository;
    }

    public async Task<bool> LoginExists(string login)
        => await _userRepository.LoginExists(login);

    public void AssignPasswordHash(User user, string password)
    {
        var passwordHasher = new PasswordHasher<User>();
        user.HashedPassword = passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password)
    {
        var passwordHasher = new PasswordHasher<User>();
        return passwordHasher.VerifyHashedPassword(user, user.HashedPassword, password) is PasswordVerificationResult
            .Success;
    }

    public async Task InsertUser(User user, string token)
        => await _userRepository.InsertUser(user, token);
    
    public async Task<User> GetUserByLogin(string login)
        => await _userRepository.GetUserByLogin(login);
    
    public async Task<User> GetPostUserDataById(long id)
        => await _userRepository.GetPostUserDataById(id);
    
    public async Task<string> GetUserHashedPassword(string login)
        => await _userRepository.GetUserHashedPassword(login);

    public async Task EditProfile(int id, EditProfileRequest profile)
        => await _userRepository.EditProfile(id, profile);   

    public async Task<User> GetProfileById(int id)
        => await _userRepository.GetProfileById(id);
    
    public string CreateToken(User user)
    {
        var claims = new List<Claim> {
            new(ClaimTypes.UserData, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Value.Token));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler().WriteToken(token);
        return handler;
    }
    
    public int GetIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        return int.Parse(jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.UserData).Value);
    }
}