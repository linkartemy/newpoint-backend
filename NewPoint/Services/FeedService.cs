using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Extensions;
using NewPoint.Handlers;
using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class FeedService : GrpcFeed.GrpcFeedBase
{
    private readonly ILogger<ArticleService> _logger;
    private readonly IArticleRepository _articleRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IArticleCommentRepository _articleCommentRepository;

    public FeedService(IUserRepository userRepository, IArticleRepository articleRepository, IPostRepository postRepository, IArticleCommentRepository articleCommentRepository, ILogger<ArticleService> logger)
    {
        _userRepository = userRepository;
        _articleRepository = articleRepository;
        _postRepository = postRepository;
        _articleCommentRepository = articleCommentRepository;
        _logger = logger;
    }

    public override async Task<Response> GetFeedByUserId(GetFeedByUserIdRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var lastArticleId = request.LastArticleId;
            var lastPostId = request.LastPostId;
            if (lastArticleId == -1)
            {
                lastArticleId = await _articleRepository.GetMaxId();
            }
            if (lastPostId == -1)
            {
                lastPostId = await _postRepository.GetMaxId();
            }
            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);
            if (user == null)
            {
                response.Status = 404;
                response.Error = "User not found";
                return response;
            }
            var feedElements = new List<FeedElement>();
            var articles = await _articleRepository.GetArticlesFromId(lastArticleId);
            var posts = await _postRepository.GetPostsFromId(lastPostId);
            foreach (var article in articles)
            {
                var author = await _userRepository.GetPostUserDataById(article.AuthorId);
                if (author is null)
                {
                    article.Login = "Unknown";
                    article.Name = "Unknown";
                    article.Surname = "";
                }
                else
                {
                    article.Login = author.Login;
                    article.Name = author.Name;
                    article.Surname = author.Surname;
                    article.ProfileImageId = author.ProfileImageId;
                }
                article.Liked = await _articleRepository.IsLikedByUser(article.Id, (await _userRepository.GetUserByToken(token)).Id);
                feedElements.Add(new FeedElement
                {
                    Article = article.ToArticleModel().ToNullableArticle(),
                    Post = new NullablePost { Null = new NullValue() }
                });
            }
            foreach (var post in posts)
            {
                var author = await _userRepository.GetPostUserDataById(post.AuthorId);
                if (author is null)
                {
                    post.Login = "Unknown";
                    post.Name = "Unknown";
                    post.Surname = "";
                }
                else
                {
                    post.Login = author.Login;
                    post.Name = author.Name;
                    post.Surname = author.Surname;
                    post.ProfileImageId = author.ProfileImageId;
                }
                post.Liked = await _postRepository.IsLikedByUser(post.Id, (await _userRepository.GetUserByToken(token)).Id);
                feedElements.Add(new FeedElement
                {
                    Article = new NullableArticle { Null = new NullValue() },
                    Post = post.ToPostModel().ToNullablePost()
                });
            }
            feedElements = feedElements.OrderByDescending(feedElement =>
            feedElement.Article.KindCase == NullableArticle.KindOneofCase.Null ?
            feedElement.Post.Data.CreationTimestamp :
            feedElement.Article.Data.CreationTimestamp).ToList();
            response.Data = Any.Pack(new GetFeedByUserIdResponse
            {
                Feed = { feedElements }
            });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }
}