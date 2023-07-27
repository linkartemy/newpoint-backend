using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<Comment>> GetCommentsByPostId(long postId)
        => await _commentRepository.GetCommentsByPostId(postId);

    public async Task<bool> IsLikedByUser(long commentId, long userId)
        => await _commentRepository.IsLikedByUser(commentId, userId);

    public async Task Like(long commentId, long userId)
    {
        await _commentRepository.SetLikesById(commentId, await _commentRepository.GetLikesById(commentId) + 1);
        await _commentRepository.InsertCommentLike(commentId, userId);
    }

    public async Task UnLike(long commentId, long userId)
    {
        await _commentRepository.SetLikesById(commentId, await _commentRepository.GetLikesById(commentId) - 1);
        await _commentRepository.DeleteCommentLike(commentId, userId);
    }
}