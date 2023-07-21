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
}