using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class PostService : IPostService
{
    private IPostRepository _postRepository;

    public PostService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }
    
    public async Task<List<Post>> GetPosts()
    {
        return await _postRepository.GetPosts();
    }
}