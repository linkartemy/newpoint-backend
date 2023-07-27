using NewPoint.Models;

namespace NewPoint.Services;

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetCommentsByPostId(long id);
    Task<bool> IsLikedByUser(long id, long userId);
    Task Add(long id, long userId, string content);
    Task Like(long id, long userId);
    Task UnLike(long id, long userId);
}
