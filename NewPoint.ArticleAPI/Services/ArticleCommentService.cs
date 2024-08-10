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

    public override async Task<Response> GetCommentsByArticleId(GetCommentsByArticleIdRequest request,
        ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
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

            response.Data = Any.Pack(new GetCommentsByArticleIdResponse { Comments = { comments } });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> AddComment(AddArticleCommentRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = context.UserState["user"] as User;

            await _articleRepository.SetCommentsById(request.ArticleId,
                await _articleRepository.GetCommentsById(request.ArticleId) + 1);
            await _articleCommentRepository.Insert(request.ArticleId, user.Id, request.Content.Trim());

            response.Data = Any.Pack(new AddArticleCommentResponse
            {
                Added = true
            });

            return response;
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            response.Status = 500;
            return response;
        }
    }

    public override async Task<Response> DeleteComment(DeleteArticleCommentRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = context.UserState["user"] as User;

            var comment = await _articleCommentRepository.GetCommentById(request.CommentId);
            if (comment == null)
            {
                response.Data = Any.Pack(new DeleteArticleCommentResponse
                {
                    Deleted = true
                });

                return response;
            }

            await _articleRepository.SetCommentsById(comment.ArticleId,
                await _articleRepository.GetCommentsById(comment.ArticleId) - 1);
            await _articleCommentRepository.DeleteCommentLikes(request.CommentId);
            await _articleCommentRepository.Delete(request.CommentId);

            response.Data = Any.Pack(new DeleteArticleCommentResponse
            {
                Deleted = true
            });

            return response;
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            response.Status = 500;
            return response;
        }
    }

    public override async Task<Response> LikeComment(LikeArticleCommentRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = context.UserState["user"] as User;

            await _articleCommentRepository.SetLikesById(request.CommentId,
                await _articleCommentRepository.GetLikesById(request.CommentId) + 1);
            await _articleCommentRepository.InsertCommentLike(request.CommentId, user.Id);

            response.Data = Any.Pack(new LikeArticleCommentResponse
            {
                Liked = true
            });

            return response;
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            response.Status = 500;
            return response;
        }
    }

    public override async Task<Response> UnLikeComment(UnLikeArticleCommentRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = context.UserState["user"] as User;

            await _articleCommentRepository.SetLikesById(request.CommentId,
                await _articleCommentRepository.GetLikesById(request.CommentId) - 1);
            await _articleCommentRepository.DeleteCommentLike(request.CommentId, user.Id);

            response.Data = Any.Pack(new UnLikeArticleCommentResponse
            {
                Liked = false
            });

            return response;
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            response.Status = 500;
            return response;
        }
    }
}