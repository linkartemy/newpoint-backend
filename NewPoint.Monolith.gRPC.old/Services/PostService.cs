using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Extensions;
using NewPoint.Handlers;
using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class PostService : GrpcPost.GrpcPostBase
{
    private readonly ILogger<PostService> _logger;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICommentRepository _commentRepository;

    public PostService(IUserRepository userRepository, IPostRepository postRepository, ICommentRepository commentRepository, ILogger<PostService> logger)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public override async Task<Response> AddPost(AddPostRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var authorId = request.AuthorId;
            var content = request.Content.Trim();
            var id = await _postRepository.AddPost(authorId, content);
            response.Data = Any.Pack(new AddPostResponse { Id = id });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> GetPosts(GetPostsRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var posts = (await _postRepository.GetPosts()).OrderByDescending(post => post.CreationTimestamp).Select(
                async post =>
                {
                    var postAuthor = await _userRepository.GetPostUserDataById(post.AuthorId);
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

            response.Data = Any.Pack(new GetPostsResponse { Posts = { posts } });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> GetPostsByUserId(GetPostsByUserIdRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = await _userRepository.GetPostUserDataById(request.UserId);
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

            response.Data = Any.Pack(new GetPostsByUserIdResponse { Posts = { posts } });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> GetPostById(GetPostByIdRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var post = await _postRepository.GetPost(request.Id);

            var user = await _userRepository.GetPostUserDataById(post.AuthorId);
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

            response.Data = Any.Pack(new GetPostByIdResponse { Post = post.ToPostModel() });

            return response;
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            response.Status = 500;
            return response;
        }
    }

    public override async Task<Response> LikePost(LikePostRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = context.RetrieveUser();

            await _postRepository.SetLikesById(request.PostId, await _postRepository.GetLikesById(request.PostId) + 1);
            await _postRepository.InsertPostLike(request.PostId, user.Id);

            response.Data = Any.Pack(new LikePostResponse
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

    public override async Task<Response> UnLikePost(UnLikePostRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var user = context.RetrieveUser();

            await _postRepository.SetLikesById(request.PostId, await _postRepository.GetLikesById(request.PostId) - 1);
            await _postRepository.DeletePostLike(request.PostId, user.Id);

            response.Data = Any.Pack(new UnLikePostResponse
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

    public override async Task<Response> SharePost(SharePostRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            await _postRepository.SetSharesById(request.PostId,
                await _postRepository.GetSharesById(request.PostId) + 1);

            response.Data = Any.Pack(new SharePostResponse
            {
                Shared = true
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

    public override async Task<Response> AddPostView(AddPostViewRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var views = await _postRepository.GetPostViewsById(request.PostId) + 1;

            await _postRepository.SetPostViewsById(request.PostId,
                views);

            response.Data = Any.Pack(new AddPostViewResponse
            {
                Views = views
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

    public override async Task<Response> DeletePost(DeletePostRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
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

            response.Data = Any.Pack(new DeletePostResponse
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
}