using NewPoint.Models;

namespace NewPoint.Repositories;

public interface IUserRepository
{
    Task<bool> LoginExists(string login);
    Task InsertUser(User user);
    Task<User> GetUserByLogin(string login);
    Task<string> GetUserHashedPassword(string login);
}
