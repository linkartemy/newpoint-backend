using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class ArticleShareRepository : IArticleShareRepository
{
    public static string TableName = "article_share";

    public async Task<long> AddArticleShare(long userId, long articleId)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>($@"
        INSERT INTO {TableName} (user_id, article_id, creation_timestamp)
        VALUES (@userId, @articleId, now())
        RETURNING id;
        ",
            new
            {
                userId,
                articleId
            });
        return id;
    }

    public async Task DeleteArticleShareByArticleId(long articleId)
    {
        await DatabaseHandler.Connection.ExecuteAsync($@"
        DELETE FROM {TableName} WHERE id = @articleId;
        ",
            new
            {
                articleId
            });
    }
}

public interface IArticleShareRepository
{
    Task<long> AddArticleShare(long userId, long articleId);
    Task DeleteArticleShareByArticleId(long articleId);
}