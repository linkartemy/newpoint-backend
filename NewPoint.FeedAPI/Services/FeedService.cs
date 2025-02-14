using Grpc.Core;
using NewPoint.Common.Extensions;
using NewPoint.Common.Models;
using NewPoint.FeedAPI.Clients;

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
        private readonly IArticleClient _articleClient;
        private readonly IPostClient _postClient;
        private readonly IUserClient _userClient;

        public FeedService(
            IArticleClient articleClient,
            IPostClient postClient,
            IUserClient userClient,
            ILogger<FeedService> logger)
        {
            _userClient = userClient;
            _articleClient = articleClient;
            _postClient = postClient;
            _logger = logger;
        }

        public override async Task<GetFeedByUserIdResponse> GetFeedByUserId(GetFeedByUserIdRequest request, ServerCallContext context)
        {
            try
            {
                var user = context.RetrieveUser();
                var token = context.RetrieveToken();

                var articlePageSize = request.ArticlePagination?.PageSize ?? 10;
                var articleCursorCreatedAt = request.ArticlePagination?.CursorCreatedAt?.ToDateTime();
                var articleCursorId = request.ArticlePagination?.CursorId > 0 ? request.ArticlePagination.CursorId : (long?)null;

                var postPageSize = request.PostPagination?.PageSize ?? 10;
                var postCursorCreatedAt = request.PostPagination?.CursorCreatedAt?.ToDateTime();
                var postCursorId = request.PostPagination?.CursorId > 0 ? request.PostPagination.CursorId : (long?)null;

                var feedElements = new List<FeedElement>();

                var getArticlesResponse = await _articleClient.GetArticles(token, articlePageSize, articleCursorCreatedAt, articleCursorId);
                var articles = getArticlesResponse.Articles.ToList();
                var hasMoreArticles = articles.Count > articlePageSize;
                var paginatedArticles = articles.Take(articlePageSize).ToList();

                foreach (var article in paginatedArticles)
                {
                    var author = await _userClient.GetPostUserDataById(article.AuthorId, token) ?? new User
                    {
                        Login = "Unknown",
                        Name = "Unknown",
                        Surname = "",
                        ProfileImageId = 0
                    };

                    article.Liked = (await _articleClient.IsArticleLikedByUser(token, user.Id, article.Id)).Liked;

                    feedElements.Add(new FeedElement
                    {
                        Article = new ArticleModel
                        {
                            Id = article.Id,
                            AuthorId = article.AuthorId,
                            Login = author.Login,
                            Name = author.Name,
                            Surname = author.Surname,
                            ProfileImageId = author.ProfileImageId,
                            Title = article.Title,
                            Content = article.Content,
                            Images = article.Images,
                            Likes = article.Likes,
                            Shares = article.Shares,
                            Comments = article.Comments,
                            Views = article.Views,
                            Liked = article.Liked,
                            Bookmarked = false, // Implement if needed
                            CreationTimestamp = article.CreationTimestamp
                        }
                    });
                }

                var getPostsResponse = await _postClient.GetPosts(token, postPageSize, postCursorCreatedAt, postCursorId);
                var posts = getPostsResponse.Posts.ToList();
                var hasMorePosts = posts.Count > postPageSize;
                var paginatedPosts = posts.Take(postPageSize).ToList();

                foreach (var post in paginatedPosts)
                {
                    var author = await _userClient.GetPostUserDataById(post.AuthorId, context.RetrieveToken()) ?? new User
                    {
                        Login = "Unknown",
                        Name = "Unknown",
                        Surname = "",
                        ProfileImageId = 0
                    };

                    post.Liked = (await _postClient.IsPostLikedByUser(token, user.Id, post.Id)).Liked;

                    feedElements.Add(new FeedElement
                    {
                        Post = new PostModel
                        {
                            Id = post.Id,
                            AuthorId = post.AuthorId,
                            Login = author.Login,
                            Name = author.Name,
                            Surname = author.Surname,
                            ProfileImageId = author.ProfileImageId,
                            Content = post.Content,
                            Images = post.Images,
                            Likes = post.Likes,
                            Shares = post.Shares,
                            Comments = post.Comments,
                            Views = post.Views,
                            Liked = post.Liked,
                            Bookmarked = false, // Implement if needed
                            CreationTimestamp = post.CreationTimestamp
                        }
                    });
                }

                feedElements = feedElements.OrderByDescending(f => f.ContentCase == FeedElement.ContentOneofCase.Post
                    ? f.Post.CreationTimestamp.Seconds
                    : f.Article.CreationTimestamp.Seconds).ToList();

                var nextArticleCursorCreatedAt = getArticlesResponse.HasMore ? paginatedArticles.Last().CreationTimestamp : null;
                var nextArticleCursorId = hasMoreArticles ? paginatedArticles.Last().Id : 0;

                var nextPostCursorCreatedAt = hasMorePosts ? paginatedPosts.Last().CreationTimestamp : null;
                var nextPostCursorId = hasMorePosts ? paginatedPosts.Last().Id : 0;

                return new GetFeedByUserIdResponse
                {
                    Feed = { feedElements },
                    ArticleNextPagination = new FeedNextPagination
                    {
                        NextCursorCreatedAt = nextArticleCursorCreatedAt,
                        NextCursorId = nextArticleCursorId,
                        HasMore = hasMoreArticles
                    },
                    PostNextPagination = new FeedNextPagination
                    {
                        NextCursorCreatedAt = nextPostCursorCreatedAt,
                        NextCursorId = nextPostCursorId,
                        HasMore = hasMorePosts
                    }
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
