using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Handlers;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class ArticleCommentService : GrpcArticleComment.GrpcArticleCommentBase
{
    private readonly IArticleCommentRepository _articleCommentRepository;
    private readonly ILogger<ArticleService> _logger;
    private readonly IArticleRepository _articleRepository;
    private readonly IUserRepository _userRepository;

    public ArticleCommentService(IUserRepository userRepository, IArticleRepository articleRepository,
        IArticleCommentRepository commentRepository, ILogger<ArticleService> logger)
    {
        _userRepository = userRepository;
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
                        var user = await _userRepository.GetPostUserDataById(comment.UserId);
                        if (user is null)
                        {
                            comment.Login = "Unknown";
                            comment.Name = "Unknown";
                            comment.Surname = "";
                        }
                        else
                        {
                            comment.Login = user.Login;
                            comment.Name = user.Name;
                            comment.Surname = user.Surname;
                        }

                        var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
                        comment.Liked =
                            await _articleCommentRepository.IsLikedByUser(comment.Id,
                                (await _userRepository.GetUserByToken(token)).Id);

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
            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);
            if (user == null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

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
            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);
            if (user == null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

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
            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);
            if (user == null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

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
            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);
            if (user == null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

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