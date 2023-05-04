using NewPoint.Models;

namespace NewPoint.Services;

public interface IUserService
{
    Task<bool> LoginExists(string login);
    void AssignPasswordHash(User user, string password);
    bool VerifyPassword(User user, string password);
    Task InsertUser(User user);
    Task<User> GetUserByLogin(string login);
    Task<string> GetUserHashedPassword(string login);
    string CreateToken(User user);
}