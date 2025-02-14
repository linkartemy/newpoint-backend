using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.FeedAPI.Extensions;

namespace NewPoint.FeedAPI.Clients;

public class ArticleClient : IArticleClient
{
    public GrpcChannel Channel { get; private set; }
    private readonly GrpcArticle.GrpcArticleClient Client;
    public string Url { get; set; } = "http://newpoint-article-service:5140";

    public ArticleClient()
    {
        Channel = GrpcChannel.ForAddress(Url);
        Client = new GrpcArticle.GrpcArticleClient(Channel);
    }

    public async Task<GetArticlesResponse> GetArticles(string userToken, int pageSize, DateTime? cursorCreatedAt, long? cursorId)
    {
        try
        {
            var request = new GetArticlesRequest
            {
                PageSize = pageSize
            };
            if (cursorCreatedAt != null)
            {
                request.CursorCreatedAt = DateTimeHandler.DateTimeToTimestamp(cursorCreatedAt.Value);
            }
            if (cursorId != null)
            {
                request.CursorId = cursorId.Value;
            }
            return await Client.GetArticlesAsync(request, new Metadata {
                {"Authorization", $"Bearer {userToken}"},
            });
        }
        catch (Exception e) {}
        return new GetArticlesResponse();
    }

    public async Task<IsArticleLikedByUserResponse> IsArticleLikedByUser(string userToken, long userId, long articleId)
    {
        try
        {
            var request = new IsArticleLikedByUserRequest
            {
                UserId = userId,
                ArticleId = articleId
            };
            return await Client.IsArticleLikedByUserAsync(request, new Metadata {
                {"Authorization", $"Bearer {userToken}"},
            });
        }
        catch (Exception e) {}
        return new IsArticleLikedByUserResponse();
    }
}

public interface IArticleClient
{
    Task<GetArticlesResponse> GetArticles(string userToken, int pageSize, DateTime? cursorCreatedAt, long? cursorId);
    Task<IsArticleLikedByUserResponse> IsArticleLikedByUser(string userToken, long userId, long articleId);
}