using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;
using NewPoint.Models.Requests;

namespace NewPoint.Repositories;

internal class UserRepository : IUserRepository
{
    public async Task<bool> LoginExists(string login)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@"
        SELECT COUNT(1) FROM ""user""
        WHERE login=@login;
        ",
            new { login });

        return counter != 0;
    }

    public async Task InsertUser(User user, string token)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@"
        INSERT INTO ""user"" (login, password_hash, name, surname, email, phone, date_of_birth, last_login_timestamp, ip, token, registration_timestamp)
        VALUES (@login, @passwordHash, @name, @surname, @email, @phone, @dateOfBirth, @lastLoginTimestamp, @ip, @token, now())
        RETURNING id;
        ",
            new
            {
                login = user.Login,
                passwordHash = user.HashedPassword,
                name = user.Name,
                surname = user.Surname,
                email = user.Email,
                phone = user.Phone,
                dateOfBirth = user.BirthDate,
                lastLoginTimestamp = user.LastLoginTimeStamp,
                ip = user.IP,
                token
            });
        user.Id = id;
    }
    
    public async Task<User> GetUserByLogin(string login)
    {
        var user = DatabaseHandler.Connection.QueryFirst<User>(@"
        SELECT
            *
        FROM ""user""
        WHERE login=@login;
        ",
            new { login });

        return user;
    }
    
    public async Task<User> GetPostUserDataById(long id)
    {
        var user = DatabaseHandler.Connection.QueryFirst<User>(@"
        SELECT
            login, name, surname
        FROM ""user""
        WHERE id=@id;
        ",
            new { id });

        return user;
    }
    
    public async Task<string> GetUserHashedPassword(string login)
    {
        var hashedPassword = DatabaseHandler.Connection.QueryFirst<string>(@"
        SELECT
            password_hash
        FROM ""user""
        WHERE login=@login;
        ",
            new { login });

        return hashedPassword;
    }

    public async Task EditProfile(int id, EditProfileRequest user)
    {
        await DatabaseHandler.Connection.ExecuteAsync(@"
        UPDATE
            ""user""
        SET name=@name,
            surname=@surname,
            description=@description,
            birthdate=@birthdate,
            location=@location
        WHERE id=@id;
        ",
            new {id=id, name=user.Name, surname=user.Surname, description=user.Description,birthdate=user.BirthDate,location=user.Location });
    }
    
    public async Task<User> GetProfileById(int id)
    {
        var user = await DatabaseHandler.Connection.QueryFirstAsync<User>(@"
        SELECT * FROM
            ""user""
        WHERE id=@id;
        ",
            new {id=id});
        return user;
    }
}