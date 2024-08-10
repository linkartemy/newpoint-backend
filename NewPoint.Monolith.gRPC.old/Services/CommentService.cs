using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Extensions;
using NewPoint.Handlers;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class CommentService : GrpcComment.GrpcCommentBase
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<CommentService> _logger;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;

    public CommentService(IUserRepository userRepository, IPostRepository postRepository,
        ICommentRepository commentRepository, ILogger<CommentService> logger)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public override async Task<Response> GetCommentsByPostId(GetCommentsByPostIdRequest request,
        ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var lastCommentId = request.LastCommentId;
            if (lastCommentId == -1)
            {
                lastCommentId = long.MaxValue;
            }
            var comments = (await _commentRepository.GetCommentsByPostIdFromId(request.PostId, lastCommentId))
                .OrderByDescending(comment => comment.CreationTimestamp).Select(
                    async comment =>
                    {
                        var commentAuthor = await _userRepository.GetPostUserDataById(comment.UserId);
                        if (commentAuthor is null)
                        {
                            comment.Login = "Unknown";
                            comment.Name = "Unknown";
                            comment.Surname = "";
                        }
                        else
                        {
                            comment.Login = commentAuthor.Login;
                            comment.Name = commentAuthor.Name;
                            comment.Surname = commentAuthor.Surname;
                        }

                        var user = context.RetrieveUser();
                        comment.Liked =
                            await _commentRepository.IsLikedByUser(comment.Id,
                                user.Id);

                        return comment.ToCommentModel();
                    }).Select(comment => comment.Result).ToList();

            response.Data = Any.Pack(new GetCommentsByPostIdResponse { Comments = { comments } });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> GetCommentById(GetCommentByIdRequest request,
        ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var comment = await _commentRepository.GetCommentById(request.Id);
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
                await _commentRepository.IsLikedByUser(comment.Id,
                    (await _userRepository.GetUserByToken(token)).Id);
            response.Data = Any.Pack(new GetCommentByIdResponse { Comment = comment.ToCommentModel() });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> AddComment(AddCommentRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = context.RetrieveUser();

            await _postRepository.SetCommentsById(request.PostId,
                await _postRepository.GetCommentsById(request.PostId) + 1);
            await _commentRepository.Insert(request.PostId, user.Id, request.Content.Trim());

            response.Data = Any.Pack(new AddCommentResponse
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

    public override async Task<Response> DeleteComment(DeleteCommentRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var comment = await _commentRepository.GetCommentById(request.CommentId);
            if (comment == null)
            {
                response.Data = Any.Pack(new DeleteCommentResponse
                {
                    Deleted = true
                });

                return response;
            }

            await _postRepository.SetCommentsById(comment.PostId,
                await _postRepository.GetCommentsById(comment.PostId) - 1);
            await _commentRepository.DeleteCommentLikes(request.CommentId);
            await _commentRepository.Delete(request.CommentId);

            response.Data = Any.Pack(new DeleteCommentResponse
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

    public override async Task<Response> LikeComment(LikeCommentRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = context.RetrieveUser();

            await _commentRepository.SetLikesById(request.CommentId,
                await _commentRepository.GetLikesById(request.CommentId) + 1);
            await _commentRepository.InsertCommentLike(request.CommentId, user.Id);

            response.Data = Any.Pack(new LikeCommentResponse
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

    public override async Task<Response> UnLikeComment(UnLikeCommentRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = context.RetrieveUser();

            await _commentRepository.SetLikesById(request.CommentId,
                await _commentRepository.GetLikesById(request.CommentId) - 1);
            await _commentRepository.DeleteCommentLike(request.CommentId, user.Id);

            response.Data = Any.Pack(new UnLikeCommentResponse
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