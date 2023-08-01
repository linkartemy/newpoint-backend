using System.Text.RegularExpressions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint;
using NewPoint.Handlers;
using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class CommentService : GrpcComment.GrpcCommentBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<PostService> _logger;

    public CommentService(IUserRepository userRepository, IPostRepository postRepository,
        ICommentRepository commentRepository, ILogger<PostService> logger)
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
            Status = 200,
        };
        try
        {
            var comments = (await _commentRepository.GetCommentsByPostId(request.PostId))
                .OrderByDescending(post => post.CreationTimestamp).Select(
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
                            await _commentRepository.IsLikedByUser(comment.Id,
                                (await _userRepository.GetUserByToken(token)).Id);

                        return new CommentModel
                        {
                            Id = comment.Id, Login = comment.Login, Name = comment.Name, Surname = comment.Surname,
                            Content = comment.Content, Likes = comment.Likes, Liked = comment.Liked,
                            CreationTimestamp = DateTimeHandler.TimestampToDateTime(comment.CreationTimestamp)
                        };
                        ;
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

    public override async Task<Response> AddComment(AddCommentRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200,
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

            await _commentRepository.Insert(request.PostId, user.Id, request.Content);

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

    public override async Task<Response> LikeComment(LikeCommentRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200,
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

            await _commentRepository.SetLikesById(request.CommentId, await _commentRepository.GetLikesById(request.CommentId) + 1);
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
            Status = 200,
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

            await _commentRepository.SetLikesById(request.CommentId, await _commentRepository.GetLikesById(request.CommentId) - 1);
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