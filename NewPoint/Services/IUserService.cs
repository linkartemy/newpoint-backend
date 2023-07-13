using NewPoint.Models;
using NewPoint.Models.Requests;

namespace NewPoint.Services;

public interface IUserService
{
    Task<bool> LoginExists(string login);
    void AssignPasswordHash(User user, string password);
    bool VerifyPassword(User user, string password);
    Task InsertUser(User user, string token);
    Task<User> GetUserByLogin(string login);
    Task<string> GetTokenById(long id);
    Task<User?> GetUserByToken(string token);
    Task<User?> GetPostUserDataById(long id);
    public Task<User> GetProfileById(int id);
    Task<string> GetUserHashedPassword(string login);
    Task EditProfile(int id, EditProfileRequest user);
    string CreateToken(User user);
    int GetIdFromToken(string token);
}