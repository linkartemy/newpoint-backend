using NewPoint.Models;
using NewPoint.Models.Requests;

namespace NewPoint.Repositories;

public interface IUserRepository
{
    Task<bool> LoginExists(string login);
    Task InsertUser(User user);
    Task<User> GetUserByLogin(string login);
    Task<string> GetUserHashedPassword(string login);
    Task EditProfile(int id, EditProfileRequest user);
    public Task<User> GetProfileById(int id);
}
