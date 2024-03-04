using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

internal class UserRepository : IUserRepository
{
    public async Task<int> CountWithId(long id)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@"
        SELECT COUNT(1) FROM ""user""
        WHERE id=@id;
        ",
            new { id });

        return counter;
    }

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
        INSERT INTO ""user"" (login, password_hash, name, surname, email, phone, birth_date, last_login_timestamp, ip, token, registration_timestamp)
        VALUES (@login, @passwordHash, @name, @surname, @email, @phone, @birthDate, @lastLoginTimestamp, @ip, @token, now())
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
                birthDate = user.BirthDate,
                lastLoginTimestamp = user.LastLoginTimestamp,
                ip = user.IP,
                token
            });
        user.Id = id;
    }

    public async Task<User> GetUserByLogin(string login)
    {
        var user = await DatabaseHandler.Connection.QueryFirstAsync<User>(@"
        SELECT
            *
        FROM ""user""
        WHERE login=@login;
        ",
            new { login });

        return user;
    }

    public async Task<string> GetTokenById(long id)
    {
        var token = await DatabaseHandler.Connection.QueryFirstAsync<string>(@"
        SELECT
            token
        FROM ""user""
        WHERE id=@id;
        ",
            new { id });

        return token;
    }

    public async Task UpdateToken(long id, string token)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@"
        UPDATE
            ""user""
        SET token=@token
        WHERE id=@id;
        ",
            new { id, token });
    }

    public async Task<User?> GetUserByToken(string token)
    {
        var user = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<User>(@"
        SELECT
            *
        FROM ""user""
        WHERE token=@token;
        ",
            new { token });

        return user;
    }

    public async Task<User?> GetPostUserDataById(long id)
    {
        var user = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<User>(@"
        SELECT
            login, name, surname, profile_image_id AS ProfileImageId
        FROM ""user""
        WHERE id=@id;
        ",
            new { id });

        return user;
    }

    public async Task<string> GetUserHashedPassword(string login)
    {
        var hashedPassword = await DatabaseHandler.Connection.QueryFirstAsync<string>(@"
        SELECT
            password_hash
        FROM ""user""
        WHERE login=@login;
        ",
            new { login });

        return hashedPassword;
    }

    public async Task UpdateProfile(long id, User user)
    {
        await DatabaseHandler.Connection.ExecuteAsync(@"
        UPDATE
            ""user""
        SET name=@name,
            surname=@surname,
            description=@description,
            location=@location,
            birth_date=@birthDate
        WHERE id=@id;
        ",
            new
            {
                id = id,
                name = user.Name,
                surname = user.Surname,
                description = user.Description,
                location = user.Location,
                birthDate = user.BirthDate
            });
    }

    public async Task UpdateProfileImageId(long id, long profileImageId)
    {
        await DatabaseHandler.Connection.ExecuteAsync(@"
        UPDATE
            ""user""
        SET profile_image_id=@profileImageId
        WHERE id=@id;
        ",
            new
            {
                id = id,
                profileImageId = profileImageId
            });
    }

    public async Task<User?> GetProfileById(long id)
    {
        var profile = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<User>(@"
        SELECT
            login AS Login,
            name AS Name,
            surname AS Surname,
            profile_image_id AS ProfileImageId,
            header_image_id AS HeaderImageId,
            description AS Description,
            location AS Location,
            email as Email,
            phone as Phone,
            birth_date as BirthDate,
            registration_timestamp as RegistrationTimestamp,
            followers as Followers,
            following as Following
        FROM ""user""
        WHERE id=@id;
        ",
            new { id });
        return profile;
    }

    public async Task<int> GetFollowingByUserId(long userId)
    {
        var following = await DatabaseHandler.Connection.QueryFirstAsync<int>(@"
        SELECT
            following
        FROM ""user""
        WHERE id=@userId;
        ",
            new { userId });

        return following;
    }

    public async Task UpdateFollowingByUserId(long userId, int following)
    {
        await DatabaseHandler.Connection.ExecuteAsync(@"
        UPDATE
            ""user""
        SET following=@following
        WHERE id=@userId;
        ",
            new
            {
                userId,
                following
            });
    }

    public async Task<int> GetFollowersByUserId(long userId)
    {
        var followers = await DatabaseHandler.Connection.QueryFirstAsync<int>(@"
        SELECT
            followers
        FROM ""user""
        WHERE id=@userId;
        ",
            new { userId });

        return followers;
    }

    public async Task UpdateFollowersByUserId(long userId, int followers)
    {
        await DatabaseHandler.Connection.ExecuteAsync(@"
        UPDATE
            ""user""
        SET followers=@followers
        WHERE id=@userId;
        ",
            new
            {
                userId,
                followers
            });
    }
}

public interface IUserRepository
{
    Task<int> CountWithId(long id);
    Task<bool> LoginExists(string login);
    Task InsertUser(User user, string token);
    Task<User> GetUserByLogin(string login);
    Task<string> GetTokenById(long id);
    Task UpdateToken(long id, string token);
    Task<User?> GetUserByToken(string token);
    Task<User?> GetPostUserDataById(long id);

    Task<string> GetUserHashedPassword(string login);

    Task UpdateProfile(long id, User user);
    Task UpdateProfileImageId(long id, long profileImageId);
    public Task<User?> GetProfileById(long id);
    public Task<int> GetFollowingByUserId(long userId);
    public Task UpdateFollowingByUserId(long userId, int following);
    public Task<int> GetFollowersByUserId(long userId);
    public Task UpdateFollowersByUserId(long userId, int followers);
}