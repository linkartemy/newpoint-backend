using NewPoint.Models;
using NewPoint.Models.Requests;

namespace NewPoint.Services;

public interface IUserService
{
    Task<bool> LoginExists(string login);
    void AssignPasswordHash(User user, string password);
    bool VerifyPassword(User user, string password);
    Task InsertUser(User user);
    Task<User> GetUserByLogin(string login);
    public Task<User> GetProfileById(int id);
    Task<string> GetUserHashedPassword(string login);
    Task EditProfile(int id, EditProfileRequest user);
    string CreateToken(User user);
    int GetIdFromToken(string token);
}