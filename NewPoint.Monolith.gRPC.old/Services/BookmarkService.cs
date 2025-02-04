using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Extensions;
using NewPoint.Handlers;
using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class BookmarkService : GrpcBookmark.GrpcBookmarkBase
{
    private readonly ILogger<BookmarkService> _logger;
    private readonly IPostBookmarkRepository _postBookmarkRepository;
    private readonly IArticleBookmarkRepository _articleBookmarkRepository;
    private readonly IPostRepository _postRepository;
    private readonly IArticleRepository _articleRepository;
    private readonly IUserRepository _userRepository;

    public BookmarkService(
        IUserRepository userRepository,
        IPostBookmarkRepository postBookmarkRepository,
        IArticleBookmarkRepository articleBookmarkRepository,
        IPostRepository postRepository, IArticleRepository articleRepository,
        ILogger<BookmarkService> logger)
    {
        _userRepository = userRepository;
        _postBookmarkRepository = postBookmarkRepository;
        _articleBookmarkRepository = articleBookmarkRepository;
        _postRepository = postRepository;
        _articleRepository = articleRepository;
        _logger = logger;
    }

    public override async Task<Response> AddPostBookmark(AddPostBookmarkRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var userId = request.UserId;
            var postId = request.PostId;
            var id = await _postBookmarkRepository.AddPostBookmark(userId, postId);
            response.Data = Any.Pack(new AddPostBookmarkResponse { Id = id });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> AddArticleBookmark(AddArticleBookmarkRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var userId = request.UserId;
            var articleId = request.ArticleId;
            var id = await _articleBookmarkRepository.AddArticleBookmark(userId, articleId);
            response.Data = Any.Pack(new AddArticleBookmarkResponse { Id = id });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> GetBookmarkedPosts(GetBookmarkedPostsRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var userId = request.UserId;
            var lastPostId = request.LastPostId;
            if (lastPostId == -1)
            {
                lastPostId = await _postBookmarkRepository.GetMaxId();
            }
            var posts = (await _postBookmarkRepository.GetPostBookmarksByUserIdFromId(userId, lastPostId))
            .OrderByDescending(postBookmark => postBookmark.CreationTimestamp)
            .Select(
                async postBookmark =>
                {
                    var post = await _postRepository.GetPost(postBookmark.ItemId) ?? new Post
                    {
                        Id = -1,
                        AuthorId = -1,
                        Content = "Deleted",
                        Images = "",
                        Likes = 0,
                        Shares = 0,
                        Comments = 0,
                        Views = 0,
                        CreationTimestamp = DateTime.UtcNow
                    };
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

                    post.Liked =
                        await _postRepository.IsLikedByUser(post.Id, userId);
                    post.Bookmarked = true;

                    return post.ToPostModel();
                }).Select(post => post.Result).ToList();

            response.Data = Any.Pack(new GetBookmarkedPostsResponse { Posts = { posts } });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> GetBookmarkedArticles(GetBookmarkedArticlesRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var userId = request.UserId;
            var lastArticleId = request.LastArticleId;
            if (lastArticleId == -1)
            {
                lastArticleId = await _articleBookmarkRepository.GetMaxId();
            }
            var articles = (await _articleBookmarkRepository.GetArticleBookmarksByUserIdFromId(userId, lastArticleId))
            .OrderByDescending(articleBookmark => articleBookmark.CreationTimestamp)
            .Select(
                async articleBookmark =>
                {
                    var article = await _articleRepository.GetArticle(articleBookmark.ItemId);
                    if (article is null)
                    {
                        article = new Article
                        {
                            Id = -1,
                            AuthorId = -1,
                            Title = "Deleted",
                            Content = "Deleted",
                            Images = "",
                            Likes = 0,
                            CreationTimestamp = DateTime.UtcNow
                        };
                    }
                    var user = await _userRepository.GetPostUserDataById(article.AuthorId);
                    if (user is null)
                    {
                        article.Login = "Unknown";
                        article.Name = "Unknown";
                        article.Surname = "";
                    }
                    else
                    {
                        article.Login = user.Login;
                        article.Name = user.Name;
                        article.Surname = user.Surname;
                        article.ProfileImageId = user.ProfileImageId;
                    }

                    article.Liked =
                        await _articleRepository.IsLikedByUser(article.Id, userId);
                    article.Bookmarked = true;

                    return article.ToArticleModel();
                }).Select(post => post.Result).ToList();

            response.Data = Any.Pack(new GetBookmarkedArticlesResponse { Articles = { articles } });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> DeletePostBookmarkByPostId(DeletePostBookmarkByPostIdRequest request, ServerCallContext context)
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

            await _postBookmarkRepository.DeletePostBookmark(user.Id, request.PostId);

            response.Data = Any.Pack(new DeletePostBookmarkByPostIdResponse
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

    public override async Task<Response> DeleteArticleBookmarkByArticleId(DeleteArticleBookmarkByArticleIdRequest request, ServerCallContext context)
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

            await _articleBookmarkRepository.DeleteArticleBookmark(user.Id, request.ArticleId);

            response.Data = Any.Pack(new DeleteArticleBookmarkByArticleIdResponse
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

    public override async Task<Response> DeleteAllBookmarksByUserId(DeleteAllBookmarksByUserIdRequest request, ServerCallContext context)
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

            await _postBookmarkRepository.DeleteAllBookmarksByUserId(user.Id);
            await _articleBookmarkRepository.DeleteAllBookmarksByUserId(user.Id);

            response.Data = Any.Pack(new DeleteAllBookmarksByUserIdResponse
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