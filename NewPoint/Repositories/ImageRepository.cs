using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

internal class ImageRepository : IImageRepository
{
    public async Task<bool> ImageExists(long id)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@"
        SELECT COUNT(1) FROM ""image""
        WHERE id=@id;
        ",
            new { id });

        return counter != 0;
    }

    public async Task<byte[]> GetImageById(long id)
    {
        var data = await DatabaseHandler.Connection.QueryFirstAsync<byte[]>(@"
        SELECT
            data
        FROM ""image""
        WHERE id=@id;
        ",
            new { id });

        return data;
    }

    public async Task<long> InsertImage(byte[] image)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@"
        INSERT INTO ""image"" (data)
        VALUES (@data)
        RETURNING id;
        ",
            new
            {
                data = image,
            });
        return id;
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

    // public async Task EditProfile(int id, EditProfileRequest user)
    // {
    //     await DatabaseHandler.Connection.ExecuteAsync(@"
    //     UPDATE
    //         ""user""
    //     SET name=@name,
    //         surname=@surname,
    //         description=@description,
    //         birthdate=@birthdate,
    //         location=@location
    //     WHERE id=@id;
    //     ",
    //         new
    //         {
    //             id = id, name = user.Name, surname = user.Surname, description = user.Description,
    //             birthdate = user.BirthDate, location = user.Location
    //         });
    // }

    public async Task<User?> GetProfileById(long id)
    {
        var profile = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<User>(@"
        SELECT
            login AS Login,
            name AS Name,
            surname AS Surname,
            profile_image AS ProfileImage,
            header_image AS HeaderImage,
            description AS Description,
            location AS Location,
            email as Email,
            phone as Phone,
            birth_date as BirthDate,
            registration_timestamp as RegistrationTimestamp
        FROM ""user""
        WHERE id=@id;
        ",
            new { id });
        return profile;
    }
}

public interface IImageRepository
{
    Task<bool> ImageExists(long id);
    Task<byte[]> GetImageById(long id);
    Task<long> InsertImage(byte[] image);
}