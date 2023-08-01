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

    public async Task InsertUser(User user, string token)
        => await _userRepository.InsertUser(user, token);

    public async Task<User> GetUserByLogin(string login)
        => await _userRepository.GetUserByLogin(login);

    public async Task<string> GetTokenById(long id)
        => await _userRepository.GetTokenById(id);
    
    public async Task UpdateToken(long id, string token)
        => await _userRepository.UpdateToken(id, token);

    public async Task<User?> GetUserByToken(string token)
        => await _userRepository.GetUserByToken(token);

    public async Task<User?> GetPostUserDataById(long id)
        => await _userRepository.GetPostUserDataById(id);

    public async Task<string> GetUserHashedPassword(string login)
        => await _userRepository.GetUserHashedPassword(login);

    public async Task EditProfile(int id, EditProfileRequest profile)
        => await _userRepository.EditProfile(id, profile);

    public async Task<User> GetProfileById(int id)
        => await _userRepository.GetProfileById(id);
}

public interface IUserService
{
    Task<bool> LoginExists(string login);
    Task InsertUser(User user, string token);
    Task<User> GetUserByLogin(string login);
    Task<string> GetTokenById(long id);
    Task UpdateToken(long id, string token);
    Task<User?> GetUserByToken(string token);
    Task<User?> GetPostUserDataById(long id);
    public Task<User> GetProfileById(int id);
    Task<string> GetUserHashedPassword(string login);
    Task EditProfile(int id, EditProfileRequest user);
}