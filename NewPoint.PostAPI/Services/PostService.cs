using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.PostAPI.Repositories;
using NewPoint.PostAPI.Clients;
using NewPoint.PostAPI.Extensions;

namespace NewPoint.PostAPI.Services;

public class PostService : GrpcPost.GrpcPostBase
{
    public static class PostServiceErrorMessages
    {
        public const string GenericError = "Something went wrong. Please try again later. We are sorry";
    }

    public static class PostServiceErrorCodes
    {
        public const string GenericError = "generic_error";
    }

    private readonly ILogger<PostService> _logger;
    private readonly IPostRepository _postRepository;
    private readonly IUserClient _userClient;
    private readonly ICommentRepository _commentRepository;

    public PostService(IUserClient userClient, IPostRepository postRepository, ICommentRepository commentRepository, ILogger<PostService> logger)
    {
        _userClient = userClient;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public override async Task<AddPostResponse> AddPost(AddPostRequest request, ServerCallContext context)
    {
        var authorId = request.AuthorId;
        var content = request.Content.Trim();
        try
        {
            var id = await _postRepository.AddPost(authorId, content);
            return new AddPostResponse { Id = id };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: PostServiceErrorCodes.GenericError,
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<GetPostsResponse> GetPosts(GetPostsRequest request, ServerCallContext context)
    {
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var cursorCreatedAt = request.CursorCreatedAt?.ToDateTime();
        var cursorId = request.CursorId > 0 ? request.CursorId : (long?)null;

        try
        {
            var posts = (await _postRepository.GetPaginatedPosts(pageSize, cursorCreatedAt, cursorId)).ToList();

            var hasMore = posts.Count > pageSize;
            var paginatedPosts = posts.Take(pageSize).ToList();

            var postsResponse = new List<PostModel>();
            foreach (var post in paginatedPosts)
            {
                var postAuthor = await _userClient.GetPostUserDataById(post.AuthorId, context.RetrieveToken()) ?? new User();

                var user = context.RetrieveUser();
                post.Liked = await _postRepository.IsLikedByUser(post.Id, user.Id);
                // TODO: Implement bookmarking
                // post.Bookmarked = await _postRepository.IsBookmarkedByUser(post.Id, user.Id);

                postsResponse.Add(new PostModel
                {
                    Id = post.Id,
                    AuthorId = post.AuthorId,
                    Login = postAuthor.Login,
                    Name = postAuthor.Name,
                    Surname = postAuthor.Surname,
                    ProfileImageId = postAuthor.ProfileImageId,
                    Content = post.Content,
                    Images = post.Images,
                    Likes = post.Likes,
                    Shares = post.Shares,
                    Comments = post.Comments,
                    Views = post.Views,
                    Liked = post.Liked,
                    // Bookmarked = post.Bookmarked, TODO: Implement bookmarking
                    CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(post.CreationTimestamp)
                });
            }

            var nextCursorCreatedAt = hasMore ? paginatedPosts.Last().CreationTimestamp : (DateTime?)null;
            var nextCursorId = hasMore ? paginatedPosts.Last().Id : 0;

            return new GetPostsResponse
            {
                Posts = { postsResponse },
                NextCursorCreatedAt = nextCursorCreatedAt != null ? DateTimeHandler.DateTimeToTimestamp(nextCursorCreatedAt.Value) : null,
                NextCursorId = nextCursorId,
                HasMore = hasMore
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: PostServiceErrorCodes.GenericError,
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<GetPostsByUserIdResponse> GetPostsByUserId(GetPostsByUserIdRequest request, ServerCallContext context)
    {
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var cursorCreatedAt = request.CursorCreatedAt?.ToDateTime();
        var cursorId = request.CursorId > 0 ? request.CursorId : (long?)null;

        try
        {
            var user = await _userClient.GetPostUserDataById(request.UserId, context.RetrieveToken()) ?? new User();

            var posts = (await _postRepository.GetPaginatedPostsByUserId(request.UserId, pageSize, cursorCreatedAt, cursorId)).ToList();

            var hasMore = posts.Count > pageSize;
            var paginatedPosts = posts.Take(pageSize).ToList();

            var postsResponse = new List<PostModel>();
            foreach (var post in paginatedPosts)
            {
                post.Login = user.Login;
                post.Name = user.Name;
                post.Surname = user.Surname;
                post.ProfileImageId = user.ProfileImageId;

                var userByToken = context.RetrieveUser();
                post.Liked = await _postRepository.IsLikedByUser(post.Id, userByToken.Id);
                // TODO: Implement bookmarking
                // post.Bookmarked = await _postRepository.IsBookmarkedByUser(post.Id, userByToken.Id);

                postsResponse.Add(new PostModel
                {
                    Id = post.Id,
                    AuthorId = post.AuthorId,
                    Login = post.Login,
                    Name = post.Name,
                    Surname = post.Surname,
                    ProfileImageId = post.ProfileImageId,
                    Content = post.Content,
                    Images = post.Images,
                    Likes = post.Likes,
                    Shares = post.Shares,
                    Comments = post.Comments,
                    Views = post.Views,
                    Liked = post.Liked,
                    // Bookmarked = post.Bookmarked, TODO: Implement bookmarking
                    CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(post.CreationTimestamp)
                });
            }

            var nextCursorCreatedAt = hasMore ? paginatedPosts.Last().CreationTimestamp : (DateTime?)null;
            var nextCursorId = hasMore ? paginatedPosts.Last().Id : 0;

            return new GetPostsByUserIdResponse
            {
                Posts = { postsResponse },
                NextCursorCreatedAt = nextCursorCreatedAt != null ? DateTimeHandler.DateTimeToTimestamp(nextCursorCreatedAt.Value) : null,
                NextCursorId = nextCursorId,
                HasMore = hasMore
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: PostServiceErrorCodes.GenericError,
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<GetPostByIdResponse> GetPostById(GetPostByIdRequest request, ServerCallContext context)
    {
        try
        {
            var post = await _postRepository.GetPost(request.Id);

            var user = await _userClient.GetPostUserDataById(post.AuthorId, context.RetrieveToken()) ?? new User();
            post.Login = user.Login;
            post.Name = user.Name;
            post.Surname = user.Surname;
            post.ProfileImageId = user.ProfileImageId;
            post.Liked = await _postRepository.IsLikedByUser(post.Id, context.RetrieveUser().Id);

            return new GetPostByIdResponse { Post = post.ToPostModel() };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: PostServiceErrorCodes.GenericError,
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<LikePostResponse> LikePost(LikePostRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.RetrieveUser();

            await _postRepository.SetLikesById(request.PostId, await _postRepository.GetLikesById(request.PostId) + 1);
            await _postRepository.InsertPostLike(request.PostId, user.Id);

            return new LikePostResponse
            {
                Liked = true
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: PostServiceErrorCodes.GenericError,
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<UnLikePostResponse> UnLikePost(UnLikePostRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.RetrieveUser();

            await _postRepository.SetLikesById(request.PostId, await _postRepository.GetLikesById(request.PostId) - 1);
            await _postRepository.DeletePostLike(request.PostId, user.Id);

            return new UnLikePostResponse
            {
                Liked = false
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: PostServiceErrorCodes.GenericError,
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<SharePostResponse> SharePost(SharePostRequest request, ServerCallContext context)
    {
        try
        {
            await _postRepository.SetSharesById(request.PostId,
                await _postRepository.GetSharesById(request.PostId) + 1);

            return new SharePostResponse
            {
                Shared = true
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: PostServiceErrorCodes.GenericError,
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<AddPostViewResponse> AddPostView(AddPostViewRequest request, ServerCallContext context)
    {
        try
        {
            var views = await _postRepository.GetPostViewsById(request.PostId) + 1;

            await _postRepository.SetPostViewsById(request.PostId,
                views);

            return new AddPostViewResponse
            {
                Views = views
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: PostServiceErrorCodes.GenericError,
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<DeletePostResponse> DeletePost(DeletePostRequest request, ServerCallContext context)
    {
        try
        {
            var comments = await _commentRepository.GetCommentsByPostId(request.PostId);
            foreach (var comment in comments)
            {
                if (comment == null) continue;
                await _commentRepository.DeleteCommentLikes(comment.Id);
                await _commentRepository.Delete(comment.Id);
            }

            await _postRepository.DeletePostLikes(request.PostId);
            await _postRepository.DeletePost(request.PostId);

            return new DeletePostResponse
            {
                Deleted = true
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: PostServiceErrorCodes.GenericError,
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<IsPostLikedByUserResponse> IsPostLikedByUser(IsPostLikedByUserRequest request, ServerCallContext context)
    {
        try
        {
            var liked = await _postRepository.IsLikedByUser(request.PostId, context.RetrieveUser().Id);

            return new IsPostLikedByUserResponse
            {
                Liked = liked
            };
        }
        catch (Exception)
        {
            throw ExceptionHandler.CreateRpcException(
                statusCode: StatusCode.Internal, errorCode: PostServiceErrorCodes.GenericError,
                message: PostServiceErrorMessages.GenericError
            );
        }
    }
}