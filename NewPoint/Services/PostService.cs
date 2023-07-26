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

    public async Task<Post> GetPost(long id)
        => await _postRepository.GetPost(id);

    public async Task<bool> IsLikedByUser(long id, long userId)
        => await _postRepository.IsLikedByUser(id, userId);

    public async Task Like(long id, long userId)
    {
        await _postRepository.SetLikesById(id, await _postRepository.GetLikesById(id) + 1);
        await _postRepository.InsertPostLike(id, userId);
    }

    public async Task UnLike(long id, long userId)
    {
        await _postRepository.SetLikesById(id, await _postRepository.GetLikesById(id) - 1);
        await _postRepository.DeletePostLike(id, userId);
    }
    
    public async Task Share(long id, long userId)
    {
        await _postRepository.SetSharesById(id, await _postRepository.GetSharesById(id) + 1);
    }
}