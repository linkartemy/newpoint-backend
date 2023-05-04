﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NewPoint.Configurations;
using NewPoint.Models;
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

    public Task InsertUser(User user)
        => _userRepository.InsertUser(user);
    
    public Task<User> GetUserByLogin(string login)
        => _userRepository.GetUserByLogin(login);
    
    public Task<string> GetUserHashedPassword(string login)
        => _userRepository.GetUserHashedPassword(login);

    public string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim> {
            new Claim(ClaimTypes.UserData, user.Id.ToString())
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
}