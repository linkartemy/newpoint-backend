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

public static class FeedServiceErrorMessages
{
    public const string GenericError = "Something went wrong. Please try again later. We are sorry";
}

public static class FeedServiceErrorCodes
{
    public const string GenericError = "generic_error";
}

namespace NewPoint.FeedAPI.Services
{
    public class FeedService : GrpcFeed.GrpcFeedBase
    {
        private readonly ILogger<FeedService> _logger;
        private readonly IArticleRepository _articleRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserClient _userClient;

        public FeedService(
            IUserClient userClient, 
            IArticleRepository articleRepository, 
            IPostRepository postRepository, 
            ILogger<FeedService> logger)
        {
            _userClient = userClient;
            _articleRepository = articleRepository;
            _postRepository = postRepository;
            _logger = logger;
        }

        public override async Task<GetFeedByUserIdResponse> GetFeedByUserId(GetFeedByUserIdRequest request, ServerCallContext context)
        {
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

                // Обработка статей
                var articles = await _articleRepository.GetArticlesFromId(lastArticleId);
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
                        Article = article.ToArticleModel()
                    });
                }

                // Обработка постов
                var posts = await _postRepository.GetPostsFromId(lastPostId);
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
                        Post = post.ToPostModel()
                    });
                }

                // Сортировка элементов фида по убыванию времени создания
                feedElements = feedElements
                    .OrderByDescending(feedElement => feedElement.ContentCase == FeedElement.ContentOneofCase.Post
                        ? feedElement.Post.CreationTimestamp
                        : feedElement.Article.CreationTimestamp)
                    .ToList();

                return new GetFeedByUserIdResponse
                {
                    Feed = { feedElements }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFeedByUserId");
                throw new RpcException(
                    new Status(StatusCode.Internal, FeedServiceErrorCodes.GenericError),
                    message: FeedServiceErrorMessages.GenericError);
            }
        }
    }
}
