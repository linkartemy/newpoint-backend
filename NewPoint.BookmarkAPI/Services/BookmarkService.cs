using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.BookmarkAPI.Repositories;
using NewPoint.BookmarkAPI.Clients;
using NewPoint.BookmarkAPI.Extensions;

namespace NewPoint.BookmarkAPI.Services;

public class BookmarkService : GrpcBookmark.GrpcBookmarkBase
{
    public static class BookmarkServiceErrorMessages
    {
        public const string GenericError = "Something went wrong. Please try again later. We are sorry";
    }

    public static class BookmarkServiceErrorCodes
    {
        public const string GenericError = "generic_error";
    }

    private readonly ILogger<BookmarkService> _logger;
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly IUserClient _userClient;

    public BookmarkService(IUserClient userClient, IBookmarkRepository bookmarkRepository, ILogger<BookmarkService> logger)
    {
        _userClient = userClient;
        _bookmarkRepository = bookmarkRepository;
        _logger = logger;
    }

    public override async Task<AddPostBookmarkResponse> AddPostBookmark(AddPostBookmarkRequest request, ServerCallContext context)
    {
        try
        {
            var id = await _bookmarkRepository.AddPostBookmark(request.UserId, request.PostId);
            return new AddPostBookmarkResponse
            {
                Id = id
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(statusCode: StatusCode.Internal, errorCode: BookmarkServiceErrorCodes.GenericError,
            message: BookmarkServiceErrorMessages.GenericError);
        }
    }

    public override async Task<AddArticleBookmarkResponse> AddArticleBookmark(AddArticleBookmarkRequest request, ServerCallContext context)
    {
        try
        {
            var id = await _bookmarkRepository.AddArticleBookmark(request.UserId, request.ArticleId);
            return new AddArticleBookmarkResponse
            {
                Id = id
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(statusCode: StatusCode.Internal, errorCode: BookmarkServiceErrorCodes.GenericError,
            message: BookmarkServiceErrorMessages.GenericError);
        }
    }

    public override async Task<GetBookmarkedPostsResponse> GetBookmarkedPosts(GetBookmarkedPostsRequest request, ServerCallContext context)
    {
        try
        {
            var token = context.RetrieveToken();
            var user = context.RetrieveUser();

            var pageSize = request.Pagination.PageSize > 0 ? request.Pagination.PageSize : 10;
            var cursorCreatedAt = request.Pagination.CursorCreatedAt?.ToDateTime();
            var cursorId = request.Pagination.CursorId > 0 ? request.Pagination.CursorId : (long?)null;

            var bookmarkedPosts = await _bookmarkRepository.GetBookmarkedPostsPaginated(request.UserId, pageSize, cursorCreatedAt, cursorId);
            var postIds = bookmarkedPosts.ToList();

            var response = new GetBookmarkedPostsResponse
            {
                PostIds = { postIds }
            };

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(statusCode: StatusCode.Internal, errorCode: BookmarkServiceErrorCodes.GenericError,
            message: BookmarkServiceErrorMessages.GenericError);
        }
    }

    public override async Task<GetBookmarkedArticlesResponse> GetBookmarkedArticles(GetBookmarkedArticlesRequest request, ServerCallContext context)
    {
        try
        {
            var token = context.RetrieveToken();
            var user = context.RetrieveUser();

            var pageSize = request.Pagination.PageSize > 0 ? request.Pagination.PageSize : 10;
            var cursorCreatedAt = request.Pagination.CursorCreatedAt?.ToDateTime();
            var cursorId = request.Pagination.CursorId > 0 ? request.Pagination.CursorId : (long?)null;

            var bookmarkedArticles = await _bookmarkRepository.GetBookmarkedArticlesPaginated(request.UserId, pageSize, cursorCreatedAt, cursorId);
            var articleIds = bookmarkedArticles.ToList();

            var response = new GetBookmarkedArticlesResponse
            {
                ArticleIds = { articleIds }
            };

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(statusCode: StatusCode.Internal, errorCode: BookmarkServiceErrorCodes.GenericError,
            message: BookmarkServiceErrorMessages.GenericError);
        }
    }

    public override async Task<DeletePostBookmarkByPostIdResponse> DeletePostBookmarkByPostId(DeletePostBookmarkByPostIdRequest request, ServerCallContext context)
    {
        try
        {
            await _bookmarkRepository.DeletePostBookmark(request.UserId, request.PostId);
            return new DeletePostBookmarkByPostIdResponse();
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(statusCode: StatusCode.Internal, errorCode: BookmarkServiceErrorCodes.GenericError,
            message: BookmarkServiceErrorMessages.GenericError);
        }
    }

    public override async Task<DeleteArticleBookmarkByArticleIdResponse> DeleteArticleBookmarkByArticleId(DeleteArticleBookmarkByArticleIdRequest request, ServerCallContext context)
    {
        try
        {
            await _bookmarkRepository.DeleteArticleBookmark(request.UserId, request.ArticleId);
            return new DeleteArticleBookmarkByArticleIdResponse();
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(statusCode: StatusCode.Internal, errorCode: BookmarkServiceErrorCodes.GenericError,
            message: BookmarkServiceErrorMessages.GenericError);
        }
    }

    public override async Task<DeleteAllBookmarksByUserIdResponse> DeleteAllBookmarksByUserId(DeleteAllBookmarksByUserIdRequest request, ServerCallContext context)
    {
        try
        {
            await _bookmarkRepository.DeleteAllBookmarksByUserId(request.UserId);
            return new DeleteAllBookmarksByUserIdResponse();
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(statusCode: StatusCode.Internal, errorCode: BookmarkServiceErrorCodes.GenericError,
            message: BookmarkServiceErrorMessages.GenericError);
        }
    }
}