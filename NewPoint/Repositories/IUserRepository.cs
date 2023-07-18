using NewPoint.Models;
using NewPoint.Models.Requests;

namespace NewPoint.Repositories;

public interface IUserRepository
{
    Task<bool> LoginExists(string login);
    Task InsertUser(User user, string token);
    Task<User> GetUserByLogin(string login);
    Task<string> GetTokenById(long id);
    Task UpdateToken(long id, string token);
    Task<User?> GetUserByToken(string token);
    Task<User?> GetPostUserDataById(long id);
    Task<string> GetUserHashedPassword(string login);
    Task EditProfile(int id, EditProfileRequest user);
    public Task<User> GetProfileById(int id);
}
