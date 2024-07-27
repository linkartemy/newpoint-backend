using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

internal class FollowRepository : IFollowRepository
{
    public readonly string TableName = "follow";

    public async Task<bool> FollowExists(long followerId, long followingId)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@$"
        SELECT COUNT(1) FROM ${TableName}
        WHERE follower_id=@followerId AND following_id=@followingId;
        ",
            new { followerId, followingId });

        return counter != 0;
    }

    public async Task InsertFollow(long followerId, long followingId)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@$"
        INSERT INTO ${TableName} (follower_id, following_id, timestamp)
        VALUES (@followerId, @followingId, now())
        RETURNING id;
        ",
            new
            {
                followerId,
                followingId
            });
    }

    public async Task<bool> DeleteFollow(long followerId, long followingId)
    {
        await DatabaseHandler.Connection.ExecuteAsync(@$"
        DELETE FROM ${TableName} WHERE follower_id = @followerId AND following_id = @followingId;
        ",
            new
            {
                followerId,
                followingId
            });
        return true;
    }

    public async Task<bool> DeleteFollowsByUserId(long userId)
    {
        await DatabaseHandler.Connection.QueryFirstAsync<string>(@$"
        DELETE FROM ${TableName} WHERE follower_id = @userId OR following_id = @userId;
        ",
            new
            {
                userId
            });
        return true;
    }
}

public interface IFollowRepository
{
    Task<bool> FollowExists(long followerId, long followingId);
    Task InsertFollow(long followerId, long followingId);
    Task<bool> DeleteFollow(long followerId, long followingId);
    Task<bool> DeleteFollowsByUserId(long userId);
}