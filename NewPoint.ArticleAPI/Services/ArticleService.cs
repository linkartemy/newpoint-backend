using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.ArticleAPI.Repositories;
using NewPoint.ArticleAPI.Clients;
using NewPoint.ArticleAPI.Extensions;

namespace NewPoint.ArticleAPI.Services;

public class ArticleService : GrpcArticle.GrpcArticleBase
{
    private readonly ILogger<ArticleService> _logger;
    private readonly IArticleRepository _articleRepository;
    private readonly IUserClient _userClient;
    private readonly IArticleCommentRepository _articleCommentRepository;

    public ArticleService(IUserClient userClient, IArticleRepository articleRepository, IArticleCommentRepository articleCommentRepository, ILogger<ArticleService> logger)
    {
        _userClient = userClient;
        _articleRepository = articleRepository;
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
            var articles = (await _articleRepository.GetArticles()).OrderByDescending(article => article.CreationTimestamp).Select(
                async article =>
                {
                    var user = await _userClient.GetPostUserDataById(article.AuthorId, context.RetrieveToken());
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

                    var userByToken = context.RetrieveUser();
                    article.Liked =
                        await _articleRepository.IsLikedByUser(article.Id, userByToken.Id);

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
                            context.RetrieveUser().Id);

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

            var user = await _userClient.GetPostUserDataById(article.AuthorId, context.RetrieveToken());
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

            article.Liked = await _articleRepository.IsLikedByUser(article.Id, context.RetrieveUser().Id);

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
            var user = context.RetrieveUser();

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
            var user = context.RetrieveUser();

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

    public override async Task<Response> IsLikedByUser(IsLikedByUserRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            response.Data = Any.Pack(new IsLikedByUserResponse
            {
                Liked = await _articleRepository.IsLikedByUser(request.ArticleId, request.UserId)
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