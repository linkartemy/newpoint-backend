using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.ArticleAPI.Repositories;
using NewPoint.ArticleAPI.Clients;
using NewPoint.ArticleAPI.Extensions;

namespace NewPoint.ArticleAPI.Services;

public class ArticleService : GrpcArticle.GrpcArticleBase
{
    public static class ArticleServiceErrorMessages
    {
        public const string GenericError = "Something went wrong. Please try again later. We are sorry";
    }

    public static class ArticleServiceErrorCodes
    {
        public const string GenericError = "generic_error";
    }

    private readonly ILogger<ArticleService> _logger;
    private readonly IArticleRepository _articleRepository;
    private readonly IUserClient _userClient;
    private readonly IArticleCommentRepository _articleCommentRepository;

    public ArticleService(IUserClient userClient, IArticleRepository articleRepository, IArticleCommentRepository articleCommentRepository, ILogger<ArticleService> logger)
    {
        _userClient = userClient;
        _articleRepository = articleRepository;
        _articleCommentRepository = articleCommentRepository;
        _logger = logger;
    }

    public override async Task<AddArticleResponse> AddArticle(AddArticleRequest request, ServerCallContext context)
    {
        var authorId = request.AuthorId;
        var title = request.Title.Trim();
        var content = request.Content.Trim();
        try
        {
            var id = await _articleRepository.AddArticle(authorId, title, content);
            return new AddArticleResponse { Id = id };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: ArticleServiceErrorCodes.GenericError,
                message: ArticleServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<GetArticlesResponse> GetArticles(GetArticlesRequest request, ServerCallContext context)
    {
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var cursorCreatedAt = request.CursorCreatedAt?.ToDateTime();
        var cursorId = request.CursorId > 0 ? request.CursorId : (long?)null;
        try
        {
            var articles = (await _articleRepository.GetArticlesPaginated(pageSize, cursorCreatedAt, cursorId)).ToList();

            var hasMore = articles.Count > pageSize;
            var paginatedArticles = articles.Take(pageSize).ToList();

            var articlesResponse = new List<ArticleModel>();
            foreach (var article in paginatedArticles)
            {
                var author = await _userClient.GetPostUserDataById(article.AuthorId, context.RetrieveToken()) ?? new User();

                var userByToken = context.RetrieveUser();
                article.Liked = await _articleRepository.IsLikedByUser(article.Id, userByToken.Id);
                // TODO: Implement bookmarking
                // article.Bookmarked = await _articleRepository.IsBookmarkedByUser(article.Id, userByToken.Id);

                articlesResponse.Add(new ArticleModel
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
                    // Bookmarked = article.Bookmarked, TODO: Implement bookmarking
                    CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(article.CreationTimestamp)
                });
            }

            var nextCursorCreatedAt = hasMore ? paginatedArticles.Last().CreationTimestamp : (DateTime?)null;
            var nextCursorId = hasMore ? paginatedArticles.Last().Id : 0;

            return new GetArticlesResponse
            {
                Articles = { articlesResponse },
                NextCursorCreatedAt = nextCursorCreatedAt != null ? DateTimeHandler.DateTimeToTimestamp(nextCursorCreatedAt.Value) : null,
                NextCursorId = nextCursorId,
                HasMore = hasMore
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: ArticleServiceErrorCodes.GenericError,
                message: ArticleServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<GetArticlesByUserIdResponse> GetArticlesByUserId(GetArticlesByUserIdRequest request, ServerCallContext context)
    {
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var cursorCreatedAt = request.CursorCreatedAt?.ToDateTime();
        var cursorId = request.CursorId > 0 ? request.CursorId : (long?)null;

        try
        {
            var user = await _userClient.GetPostUserDataById(request.UserId, context.RetrieveToken()) ?? new User();

            var articles = (await _articleRepository.GetArticlesByUserIdPaginated(request.UserId, pageSize, cursorCreatedAt, cursorId)).ToList();

            var hasMore = articles.Count > pageSize;
            var paginatedArticles = articles.Take(pageSize).ToList();

            var articlesResponse = new List<ArticleModel>();

            foreach (var article in paginatedArticles)
            {
                article.Login = user.Login;
                article.Name = user.Name;
                article.Surname = user.Surname;
                article.ProfileImageId = user.ProfileImageId;

                var userByToken = context.RetrieveUser();
                article.Liked = await _articleRepository.IsLikedByUser(article.Id, userByToken.Id);
                // TODO: Implement bookmarking
                // article.Bookmarked = await _articleRepository.IsBookmarkedByUser(article.Id, userByToken.Id);

                articlesResponse.Add(new ArticleModel
                {
                    Id = article.Id,
                    AuthorId = article.AuthorId,
                    Login = article.Login,
                    Name = article.Name,
                    Surname = article.Surname,
                    ProfileImageId = article.ProfileImageId,
                    Title = article.Title,
                    Content = article.Content,
                    Images = article.Images,
                    Likes = article.Likes,
                    Shares = article.Shares,
                    Comments = article.Comments,
                    Views = article.Views,
                    Liked = article.Liked,
                    // Bookmarked = article.Bookmarked, TODO: Implement bookmarking
                    CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(article.CreationTimestamp)
                });
            }

            var nextCursorCreatedAt = hasMore ? paginatedArticles.Last().CreationTimestamp : (DateTime?)null;
            var nextCursorId = hasMore ? paginatedArticles.Last().Id : 0;

            return new GetArticlesByUserIdResponse
            {
                Articles = { articlesResponse },
                NextCursorCreatedAt = nextCursorCreatedAt != null ? DateTimeHandler.DateTimeToTimestamp(nextCursorCreatedAt.Value) : null,
                NextCursorId = nextCursorId,
                HasMore = hasMore
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: ArticleServiceErrorCodes.GenericError,
                message: ArticleServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<GetArticleByIdResponse> GetArticleById(GetArticleByIdRequest request, ServerCallContext context)
    {
        try
        {
            var article = await _articleRepository.GetArticle(request.Id);

            var user = await _userClient.GetPostUserDataById(article.AuthorId, context.RetrieveToken()) ?? new User();
            article.Login = user.Login;
            article.Name = user.Name;
            article.Surname = user.Surname;
            article.ProfileImageId = user.ProfileImageId;
            article.Liked = await _articleRepository.IsLikedByUser(article.Id, context.RetrieveUser().Id);

            return new GetArticleByIdResponse { Article = article.ToArticleModel() };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: ArticleServiceErrorCodes.GenericError,
                message: ArticleServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<LikeArticleResponse> LikeArticle(LikeArticleRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.RetrieveUser();

            await _articleRepository.SetLikesById(request.ArticleId, await _articleRepository.GetLikesById(request.ArticleId) + 1);
            await _articleRepository.InsertArticleLike(request.ArticleId, user.Id);

            return new LikeArticleResponse
            {
                Liked = true
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: ArticleServiceErrorCodes.GenericError,
                message: ArticleServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<UnLikeArticleResponse> UnLikeArticle(UnLikeArticleRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.RetrieveUser();

            await _articleRepository.SetLikesById(request.ArticleId, await _articleRepository.GetLikesById(request.ArticleId) - 1);
            await _articleRepository.DeleteArticleLike(request.ArticleId, user.Id);

            return new UnLikeArticleResponse
            {
                Liked = false
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: ArticleServiceErrorCodes.GenericError,
                message: ArticleServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<ShareArticleResponse> ShareArticle(ShareArticleRequest request, ServerCallContext context)
    {
        try
        {
            await _articleRepository.SetSharesById(request.ArticleId,
                await _articleRepository.GetSharesById(request.ArticleId) + 1);

            return new ShareArticleResponse
            {
                Shared = true
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: ArticleServiceErrorCodes.GenericError,
                message: ArticleServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<AddArticleViewResponse> AddArticleView(AddArticleViewRequest request, ServerCallContext context)
    {
        try
        {
            var views = await _articleRepository.GetArticleViewsById(request.ArticleId) + 1;

            await _articleRepository.SetArticleViewsById(request.ArticleId,
                views);

            return new AddArticleViewResponse
            {
                Views = views
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: ArticleServiceErrorCodes.GenericError,
                message: ArticleServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<DeleteArticleResponse> DeleteArticle(DeleteArticleRequest request, ServerCallContext context)
    {
        try
        {
            var comments = await _articleCommentRepository.GetCommentsByArticleId(request.ArticleId);
            foreach (var comment in comments)
            {
                if (comment == null) continue;
                await _articleCommentRepository.DeleteCommentLikes(comment.Id);
                await _articleCommentRepository.Delete(comment.Id);
            }

            await _articleRepository.DeleteArticleLikes(request.ArticleId);
            await _articleRepository.DeleteArticle(request.ArticleId);

            return new DeleteArticleResponse
            {
                Deleted = true
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: ArticleServiceErrorCodes.GenericError,
                message: ArticleServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<IsArticleLikedByUserResponse> IsArticleLikedByUser(IsArticleLikedByUserRequest request, ServerCallContext context)
    {
        try
        {
            return new IsArticleLikedByUserResponse
            {
                Liked = await _articleRepository.IsLikedByUser(request.ArticleId, request.UserId)
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: ArticleServiceErrorCodes.GenericError,
                message: ArticleServiceErrorMessages.GenericError
            );
        }
    }
}