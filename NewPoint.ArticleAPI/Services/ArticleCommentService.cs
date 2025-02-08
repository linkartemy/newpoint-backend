using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.ArticleAPI.Repositories;
using NewPoint.ArticleAPI.Clients;
using NewPoint.Common.Extensions;

namespace NewPoint.ArticleAPI.Services;

public class ArticleCommentService : GrpcArticleComment.GrpcArticleCommentBase
{
    public static class ArticleCommentServiceErrorMessages
    {
        public const string GenericError = "Something went wrong. Please try again later. We are sorry";
    }

    public static class ArticleCommentServiceErrorCodes
    {
        public const string GenericError = "generic_error";
    }

    private readonly IArticleCommentRepository _articleCommentRepository;
    private readonly ILogger<ArticleCommentService> _logger;
    private readonly IArticleRepository _articleRepository;
    private readonly IUserClient _userClient;

    public ArticleCommentService(IUserClient userClient, IArticleRepository articleRepository,
        IArticleCommentRepository commentRepository, ILogger<ArticleCommentService> logger)
    {
        _userClient = userClient;
        _articleRepository = articleRepository;
        _articleCommentRepository = commentRepository;
        _logger = logger;
    }

    public override async Task<GetCommentsByArticleIdResponse> GetCommentsByArticleId(GetCommentsByArticleIdRequest request,
        ServerCallContext context)
    {
        try
        {
            var comments = (await _articleCommentRepository.GetCommentsByArticleId(request.ArticleId))
                .OrderByDescending(article => article.CreationTimestamp).Select(
                    async comment =>
                    {
                        var author = await _userClient.GetPostUserDataById(comment.UserId, context.RetrieveToken());
                        if (author is null)
                        {
                            comment.Login = "Unknown";
                            comment.Name = "Unknown";
                            comment.Surname = "";
                        }
                        else
                        {
                            comment.Login = author.Login;
                            comment.Name = author.Name;
                            comment.Surname = author.Surname;
                        }

                        var user = context.RetrieveUser();
                        comment.Liked =
                            await _articleCommentRepository.IsLikedByUser(comment.Id,
                                user.Id);

                        return new ArticleCommentModel
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
                        ;
                    }).Select(comment => comment.Result).ToList();

            return new GetCommentsByArticleIdResponse { Comments = { comments } };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, ArticleCommentServiceErrorCodes.GenericError),
            message: ArticleCommentServiceErrorMessages.GenericError);
        }
    }

    public override async Task<AddArticleCommentResponse> AddComment(AddArticleCommentRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.UserState["user"] as User;

            await _articleRepository.SetCommentsById(request.ArticleId,
                await _articleRepository.GetCommentsById(request.ArticleId) + 1);
            await _articleCommentRepository.Insert(request.ArticleId, user.Id, request.Content.Trim());

            return new AddArticleCommentResponse
            {
                Added = true
            };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, ArticleCommentServiceErrorCodes.GenericError),
            message: ArticleCommentServiceErrorMessages.GenericError);
        }
    }

    public override async Task<DeleteArticleCommentResponse> DeleteComment(DeleteArticleCommentRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.UserState["user"] as User;

            var comment = await _articleCommentRepository.GetCommentById(request.CommentId);
            if (comment == null)
            {
                return new DeleteArticleCommentResponse
                {
                    Deleted = true
                };
            }

            await _articleRepository.SetCommentsById(comment.ArticleId,
                await _articleRepository.GetCommentsById(comment.ArticleId) - 1);
            await _articleCommentRepository.DeleteCommentLikes(request.CommentId);
            await _articleCommentRepository.Delete(request.CommentId);

            return new DeleteArticleCommentResponse
            {
                Deleted = true
            };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, ArticleCommentServiceErrorCodes.GenericError),
            message: ArticleCommentServiceErrorMessages.GenericError);
        }
    }

    public override async Task<LikeArticleCommentResponse> LikeComment(LikeArticleCommentRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.UserState["user"] as User;

            await _articleCommentRepository.SetLikesById(request.CommentId,
                await _articleCommentRepository.GetLikesById(request.CommentId) + 1);
            await _articleCommentRepository.InsertCommentLike(request.CommentId, user.Id);

            return new LikeArticleCommentResponse
            {
                Liked = true
            };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, ArticleCommentServiceErrorCodes.GenericError),
            message: ArticleCommentServiceErrorMessages.GenericError);
        }
    }

    public override async Task<UnLikeArticleCommentResponse> UnLikeComment(UnLikeArticleCommentRequest request, ServerCallContext context)
    {
        try
        {
            var user = context.UserState["user"] as User;

            await _articleCommentRepository.SetLikesById(request.CommentId,
                await _articleCommentRepository.GetLikesById(request.CommentId) - 1);
            await _articleCommentRepository.DeleteCommentLike(request.CommentId, user.Id);

            return new UnLikeArticleCommentResponse
            {
                Liked = false
            };
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, ArticleCommentServiceErrorCodes.GenericError),
            message: ArticleCommentServiceErrorMessages.GenericError);
        }
    }
}