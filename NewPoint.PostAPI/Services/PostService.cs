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
        public const string PostNotFound = "Post not found";
        public const string Unauthorized = "User is not authorized to perform this action";
    }

    public static class PostServiceErrorCodes
    {
        public const string GenericError = "generic_error";
        public const string PostNotFound = "post_not_found";
        public const string Unauthorized = "unauthorized";
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
            throw new RpcException(
                new Status(StatusCode.Internal, PostServiceErrorCodes.GenericError),
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<GetPostsResponse> GetPosts(GetPostsRequest request, ServerCallContext context)
    {
        try
        {
            var posts = (await _postRepository.GetPosts()).OrderByDescending(post => post.CreationTimestamp).Select(
                async post =>
                {
                    var postAuthor = await _userClient.GetPostUserDataById(post.AuthorId, context.RetrieveToken());
                    if (postAuthor is null)
                    {
                        post.Login = "Unknown";
                        post.Name = "Unknown";
                        post.Surname = "";
                    }
                    else
                    {
                        post.Login = postAuthor.Login;
                        post.Name = postAuthor.Name;
                        post.Surname = postAuthor.Surname;
                        post.ProfileImageId = postAuthor.ProfileImageId;
                    }

                    var user = context.RetrieveUser();
                    post.Liked =
                        await _postRepository.IsLikedByUser(post.Id, user.Id);

                    return post.ToPostModel();
                }).Select(post => post.Result).ToList();

            return new GetPostsResponse { Posts = { posts } };
        }
        catch (Exception)
        {
            throw new RpcException(
                new Status(StatusCode.Internal, PostServiceErrorCodes.GenericError),
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<GetPostsByUserIdResponse> GetPostsByUserId(GetPostsByUserIdRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _userClient.GetPostUserDataById(request.UserId, context.RetrieveToken());
            if (user is null)
            {
                user = new User
                {
                    Login = "Unknown",
                    Name = "Unknown",
                    Surname = ""
                };
            }

            var lastPostId = request.LastPostId;
            if (lastPostId == -1)
            {
                lastPostId = await _postRepository.GetMaxId();
            }

            var posts = (await _postRepository.GetPostsFromId(lastPostId))
            .Where(post => post.AuthorId == request.UserId)
            .OrderByDescending(post => post.CreationTimestamp)
            .Select(
                async post =>
                {
                    post.Login = user.Login;
                    post.Name = user.Name;
                    post.Surname = user.Surname;
                    post.ProfileImageId = user.ProfileImageId;

                    post.Liked =
                        await _postRepository.IsLikedByUser(post.Id,
                            context.RetrieveUser().Id);

                    return post.ToPostModel();
                }).Select(post => post.Result).ToList();

            return new GetPostsByUserIdResponse { Posts = { posts } };
        }
        catch (Exception)
        {
            throw new RpcException(
                new Status(StatusCode.Internal, PostServiceErrorCodes.GenericError),
                message: PostServiceErrorMessages.GenericError
            );
        }
    }

    public override async Task<GetPostByIdResponse> GetPostById(GetPostByIdRequest request, ServerCallContext context)
    {
        try
        {
            var post = await _postRepository.GetPost(request.Id);

            var user = await _userClient.GetPostUserDataById(post.AuthorId, context.RetrieveToken());
            if (user is null)
            {
                post.Login = "Unknown";
                post.Name = "Unknown";
                post.Surname = "";
            }
            else
            {
                post.Login = user.Login;
                post.Name = user.Name;
                post.Surname = user.Surname;
                post.ProfileImageId = user.ProfileImageId;
            }

            post.Liked = await _postRepository.IsLikedByUser(post.Id, context.RetrieveUser().Id);

            return new GetPostByIdResponse { Post = post.ToPostModel() };
        }
        catch (Exception)
        {
            throw new RpcException(
                new Status(StatusCode.Internal, PostServiceErrorCodes.GenericError),
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
            throw new RpcException(
                new Status(StatusCode.Internal, PostServiceErrorCodes.GenericError),
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
            throw new RpcException(
                new Status(StatusCode.Internal, PostServiceErrorCodes.GenericError),
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
            throw new RpcException(
                new Status(StatusCode.Internal, PostServiceErrorCodes.GenericError),
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
            throw new RpcException(
                new Status(StatusCode.Internal, PostServiceErrorCodes.GenericError),
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
            throw new RpcException(
                new Status(StatusCode.Internal, PostServiceErrorCodes.GenericError),
                message: PostServiceErrorMessages.GenericError
            );
        }
    }
}