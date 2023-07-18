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
    
    public async Task<List<Post>> GetPosts()
        => (await _postRepository.GetPosts()).ToList();
    
    public async Task<Post> GetPost(long id)
        => await _postRepository.GetPost(id);
}