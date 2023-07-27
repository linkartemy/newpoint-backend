using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;

    public PostService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IEnumerable<Post>> GetPosts()
        => await _postRepository.GetPosts();

    public async Task<Post> GetPost(long postId)
        => await _postRepository.GetPost(postId);

    public async Task<bool> IsLikedByUser(long postId, long userId)
        => await _postRepository.IsLikedByUser(postId, userId);

    public async Task Like(long postId, long userId)
    {
        await _postRepository.SetLikesById(postId, await _postRepository.GetLikesById(postId) + 1);
        await _postRepository.InsertPostLike(postId, userId);
    }

    public async Task UnLike(long postId, long userId)
    {
        await _postRepository.SetLikesById(postId, await _postRepository.GetLikesById(postId) - 1);
        await _postRepository.DeletePostLike(postId, userId);
    }
    
    public async Task Share(long postId, long userId)
    {
        await _postRepository.SetSharesById(postId, await _postRepository.GetSharesById(postId) + 1);
    }
    
    public async Task<IEnumerable<Comment>> GetComments(long postId)
        => await _postRepository.GetCommentsById(postId);
    
    public async Task LikeComment(long postId, long userId)
    {
        await _postRepository.SetLikesById(postId, await _postRepository.GetLikesById(postId) + 1);
        await _postRepository.InsertPostLike(postId, userId);
    }
}