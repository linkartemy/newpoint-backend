using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.FeedAPI.Extensions;

namespace NewPoint.FeedAPI.Clients;

public class PostClient : IPostClient
{
    public GrpcChannel Channel { get; private set; }
    private readonly GrpcPost.GrpcPostClient Client;
    public string Url { get; set; } = "http://newpoint-post-service:5139";

    public PostClient()
    {
        Channel = GrpcChannel.ForAddress(Url);
        Client = new GrpcPost.GrpcPostClient(Channel);
    }

    public async Task<GetPostsResponse> GetPosts(string userToken, int pageSize, DateTime? cursorCreatedAt, long? cursorId)
    {
        try
        {
            var request = new GetPostsRequest
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
            return await Client.GetPostsAsync(request, new Metadata {
                {"Authorization", $"Bearer {userToken}"},
            });
        }
        catch (Exception e) {}
        return new GetPostsResponse();
    }

    public async Task<IsPostLikedByUserResponse> IsPostLikedByUser(string userToken, long userId, long PostId)
    {
        try
        {
            var request = new IsPostLikedByUserRequest
            {
                UserId = userId,
                PostId = PostId
            };
            return await Client.IsPostLikedByUserAsync(request, new Metadata {
                {"Authorization", $"Bearer {userToken}"},
            });
        }
        catch (Exception e) {}
        return new IsPostLikedByUserResponse();
    }
}

public interface IPostClient
{
    Task<GetPostsResponse> GetPosts(string userToken, int pageSize, DateTime? cursorCreatedAt, long? cursorId);
    Task<IsPostLikedByUserResponse> IsPostLikedByUser(string userToken, long userId, long PostId);
}