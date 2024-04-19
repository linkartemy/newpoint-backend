using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Extensions;
using NewPoint.Handlers;
using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class ArticleService : GrpcArticle.GrpcArticleBase
{
    private readonly ILogger<ArticleService> _logger;
    private readonly IArticleRepository _articleRepository;
    private readonly IArticleBookmarkRepository _articleBookmarkRepository;
    private readonly IUserRepository _userRepository;
    private readonly IArticleCommentRepository _articleCommentRepository;

    public ArticleService(IUserRepository userRepository, IArticleRepository articleRepository, IArticleBookmarkRepository articleBookmarkRepository, IArticleCommentRepository articleCommentRepository, ILogger<ArticleService> logger)
    {
        _userRepository = userRepository;
        _articleRepository = articleRepository;
        _articleBookmarkRepository = articleBookmarkRepository;
        _articleCommentRepository = articleCommentRepository;
        _logger = logger;
    }

    public override async Task<Response> AddArticle(AddArticleRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var authorId = request.AuthorId;
            var title = request.Title.Trim();
            var content = request.Content.Trim();
            var id = await _articleRepository.AddArticle(authorId, title, content);
            response.Data = Any.Pack(new AddArticleResponse { Id = id });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> GetArticles(GetArticlesRequest request, ServerCallContext context)
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
            var articles = (await _articleRepository.GetArticles()).OrderByDescending(article => article.CreationTimestamp).Select(
                async article =>
                {
                    var author = await _userRepository.GetPostUserDataById(article.AuthorId);
                    if (author is null)
                    {
                        article.Login = "Unknown";
                        article.Name = "Unknown";
                        article.Surname = "";
                    }
                    else
                    {
                        article.Login = author.Login;
                        article.Name = author.Name;
                        article.Surname = author.Surname;
                        article.ProfileImageId = author.ProfileImageId;
                    }

                    article.Liked =
                        await _articleRepository.IsLikedByUser(article.Id, user.Id);
                    article.Bookmarked = await _articleBookmarkRepository.CountArticleBookmarks(user.Id, article.Id) > 0;

                    return article.ToArticleModel();
                }).Select(article => article.Result).ToList();

            response.Data = Any.Pack(new GetArticlesResponse { Articles = { articles } });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> GetArticlesByUserId(GetArticlesByUserIdRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
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

            var lastArticleId = request.LastArticleId;
            if (lastArticleId == -1)
            {
                lastArticleId = await _articleRepository.GetMaxId();
            }

            var articles = (await _articleRepository.GetArticlesFromId(lastArticleId))
            .Where(article => article.AuthorId == request.UserId)
            .OrderByDescending(article => article.CreationTimestamp)
            .Select(
                async article =>
                {
                    article.Login = user.Login;
                    article.Name = user.Name;
                    article.Surname = user.Surname;
                    article.ProfileImageId = user.ProfileImageId;

                    article.Liked =
                        await _articleRepository.IsLikedByUser(article.Id,
                            request.UserId);
                    article.Bookmarked = await _articleBookmarkRepository.CountArticleBookmarks(request.UserId, article.Id) > 0;

                    return article.ToArticleModel();
                }).Select(article => article.Result).ToList();

            response.Data = Any.Pack(new GetArticlesByUserIdResponse { Articles = { articles } });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> GetArticleById(GetArticleByIdRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var article = await _articleRepository.GetArticle(request.Id);
            var token = context.RequestHeaders.Get("Authorization")!.Value.Split(' ')[1];
            var user = await _userRepository.GetUserByToken(token);
            if (user == null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                response.Status = 400;
                return response;
            }

            var author = await _userRepository.GetPostUserDataById(article.AuthorId);
            if (author is null)
            {
                article.Login = "Unknown";
                article.Name = "Unknown";
                article.Surname = "";
            }
            else
            {
                article.Login = author.Login;
                article.Name = author.Name;
                article.Surname = author.Surname;
                article.ProfileImageId = author.ProfileImageId;
            }

            article.Liked = await _articleRepository.IsLikedByUser(article.Id, user.Id);
            article.Bookmarked = await _articleBookmarkRepository.CountArticleBookmarks(user.Id, article.Id) > 0;

            response.Data = Any.Pack(new GetArticleByIdResponse { Article = article.ToArticleModel() });

            return response;
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            response.Status = 500;
            return response;
        }
    }

    public override async Task<Response> LikeArticle(LikeArticleRequest request, ServerCallContext context)
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

            await _articleRepository.SetLikesById(request.ArticleId, await _articleRepository.GetLikesById(request.ArticleId) + 1);
            await _articleRepository.InsertArticleLike(request.ArticleId, user.Id);

            response.Data = Any.Pack(new LikeArticleResponse
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

    public override async Task<Response> UnLikeArticle(UnLikeArticleRequest request, ServerCallContext context)
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

            await _articleRepository.SetLikesById(request.ArticleId, await _articleRepository.GetLikesById(request.ArticleId) - 1);
            await _articleRepository.DeleteArticleLike(request.ArticleId, user.Id);

            response.Data = Any.Pack(new UnLikeArticleResponse
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

    public override async Task<Response> ShareArticle(ShareArticleRequest request, ServerCallContext context)
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

            await _articleRepository.SetSharesById(request.ArticleId,
                await _articleRepository.GetSharesById(request.ArticleId) + 1);

            response.Data = Any.Pack(new ShareArticleResponse
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

    public override async Task<Response> AddArticleView(AddArticleViewRequest request, ServerCallContext context)
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

            var views = await _articleRepository.GetArticleViewsById(request.ArticleId) + 1;

            await _articleRepository.SetArticleViewsById(request.ArticleId,
                views);

            response.Data = Any.Pack(new AddArticleViewResponse
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

    public override async Task<Response> DeleteArticle(DeleteArticleRequest request, ServerCallContext context)
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

            var comments = await _articleCommentRepository.GetCommentsByArticleId(request.ArticleId);
            foreach (var comment in comments)
            {
                if (comment == null) continue;
                await _articleCommentRepository.DeleteCommentLikes(comment.Id);
                await _articleCommentRepository.Delete(comment.Id);
            }

            await _articleRepository.DeleteArticleLikes(request.ArticleId);
            await _articleRepository.DeleteArticle(request.ArticleId);

            response.Data = Any.Pack(new DeleteArticleResponse
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