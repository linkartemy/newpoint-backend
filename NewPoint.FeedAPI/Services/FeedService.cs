using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.PostAPI.Repositories;
using NewPoint.ArticleAPI.Repositories;
using NewPoint.FeedAPI.Clients;
using NewPoint.ArticleAPI.Extensions;
using NewPoint.PostAPI.Extensions;

namespace NewPoint.FeedAPI.Services;

public class FeedService : GrpcFeed.GrpcFeedBase
{
    private readonly ILogger<FeedService> _logger;
    private readonly IArticleRepository _articleRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserClient _userClient;

    public FeedService(IUserClient userClient, IArticleRepository articleRepository, IPostRepository postRepository, ILogger<FeedService> logger)
    {
        _userClient = userClient;
        _articleRepository = articleRepository;
        _postRepository = postRepository;
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
            var user = context.RetrieveUser();
            var feedElements = new List<FeedElement>();
            var articles = await _articleRepository.GetArticlesFromId(lastArticleId);
            var posts = await _postRepository.GetPostsFromId(lastPostId);
            foreach (var article in articles)
            {
                var author = await _userClient.GetPostUserDataById(article.AuthorId, context.RetrieveToken());
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
                article.Liked = await _articleRepository.IsLikedByUser(article.Id, user.Id);
                feedElements.Add(new FeedElement
                {
                    Article = article.ToArticleModel().ToNullableArticle(),
                    Post = new NullablePost { Null = new NullValue() }
                });
            }
            foreach (var post in posts)
            {
                var author = await _userClient.GetPostUserDataById(post.AuthorId, context.RetrieveToken());
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
                post.Liked = await _postRepository.IsLikedByUser(post.Id, user.Id);
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