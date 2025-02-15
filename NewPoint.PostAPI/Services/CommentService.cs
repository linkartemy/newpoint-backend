using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.PostAPI.Repositories;
using NewPoint.PostAPI.Clients;
using NewPoint.Common.Models;

namespace NewPoint.PostAPI.Services;

public class CommentService : GrpcComment.GrpcCommentBase
{
    public static class CommentServiceErrorMessages
    {
        public const string GenericError = "Something went wrong. Please try again later. We are sorry";
    }

    public static class CommentServiceErrorCodes
    {
        public const string GenericError = "generic_error";
    }

    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<CommentService> _logger;
    private readonly IPostRepository _postRepository;
    private readonly IUserClient _userClient;

    public CommentService(IUserClient userClient, IPostRepository postRepository,
        ICommentRepository commentRepository, ILogger<CommentService> logger)
    {
        _userClient = userClient;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public override async Task<GetCommentsByPostIdResponse> GetCommentsByPostId(GetCommentsByPostIdRequest request,
    ServerCallContext context)
    {
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var cursorCreatedAt = request.CursorCreatedAt?.ToDateTime();
        var cursorId = request.CursorId > 0 ? request.CursorId : (long?)null;

        try
        {
            var comments = await _commentRepository.GetCommentsByPostIdPaginated(
                request.PostId,
                pageSize,
                cursorCreatedAt,
                cursorId
            );

            var commentModels = await Task.WhenAll(comments.Select(async comment =>
            {
                var commentAuthor = await _userClient.GetPostUserDataById(comment.UserId, context.RetrieveToken()) ?? new User();
                comment.Login = commentAuthor.Login;
                comment.Name = commentAuthor.Name;
                comment.Surname = commentAuthor.Surname;

                var user = context.RetrieveUser();
                comment.Liked = await _commentRepository.IsLikedByUser(comment.Id, user.Id);

                return new CommentModel
                {
                    Id = comment.Id,
                    UserId = comment.UserId,
                    Login = comment.Login,
                    Name = comment.Name,
                    Surname = comment.Surname,
                    Content = comment.Content,
                    Likes = comment.Likes,
                    Liked = comment.Liked,
                    CreationTimestamp = DateTimeHandler.DateTimeToTimestamp(comment.CreationTimestamp)
                };
            }));

            return new GetCommentsByPostIdResponse { Comments = { commentModels } };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, CommentServiceErrorCodes.GenericError),
                message: CommentServiceErrorMessages.GenericError);
        }
    }


    public override async Task<AddCommentResponse> AddComment(AddCommentRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.RetrieveUser();

            await _postRepository.SetCommentsById(request.PostId,
                await _postRepository.GetCommentsById(request.PostId) + 1);
            await _commentRepository.Insert(request.PostId, user.Id, request.Content.Trim());

            return new AddCommentResponse
            {
                Added = true
            };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, CommentServiceErrorCodes.GenericError),
                message: CommentServiceErrorMessages.GenericError);
        }
    }

    public override async Task<DeleteCommentResponse> DeleteComment(DeleteCommentRequest request, ServerCallContext context)
    {
        try
        {
            var comment = await _commentRepository.GetCommentById(request.CommentId);
            if (comment == null)
            {
                return new DeleteCommentResponse
                {
                    Deleted = true
                };
            }

            await _postRepository.SetCommentsById(comment.PostId,
                await _postRepository.GetCommentsById(comment.PostId) - 1);
            await _commentRepository.DeleteCommentLikes(request.CommentId);
            await _commentRepository.Delete(request.CommentId);

            return new DeleteCommentResponse
            {
                Deleted = true
            };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, CommentServiceErrorCodes.GenericError),
                message: CommentServiceErrorMessages.GenericError);
        }
    }

    public override async Task<LikeCommentResponse> LikeComment(LikeCommentRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.RetrieveUser();

            await _commentRepository.SetLikesById(request.CommentId,
                await _commentRepository.GetLikesById(request.CommentId) + 1);
            await _commentRepository.InsertCommentLike(request.CommentId, user.Id);

            return new LikeCommentResponse
            {
                Liked = true
            };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, CommentServiceErrorCodes.GenericError),
                message: CommentServiceErrorMessages.GenericError);
        }
    }

    public override async Task<UnLikeCommentResponse> UnLikeComment(UnLikeCommentRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.RetrieveUser();

            await _commentRepository.SetLikesById(request.CommentId,
                await _commentRepository.GetLikesById(request.CommentId) - 1);
            await _commentRepository.DeleteCommentLike(request.CommentId, user.Id);

            return new UnLikeCommentResponse
            {
                Liked = false
            };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, CommentServiceErrorCodes.GenericError),
                message: CommentServiceErrorMessages.GenericError);
        }
    }
}