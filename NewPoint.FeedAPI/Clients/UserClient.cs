using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using NewPoint.Common.Models;
using NewPoint.FeedAPI.Extensions;

namespace NewPoint.FeedAPI.Clients;

public class UserClient : IUserClient
{
    public GrpcChannel Channel { get; private set; }
    private readonly GrpcUser.GrpcUserClient Client;
    public string Url { get; set; } = "http://newpoint-user-service:5137";

    public UserClient()
    {
        Channel = GrpcChannel.ForAddress(Url);
        Client = new GrpcUser.GrpcUserClient(Channel);
    }

    public async Task<bool> UserExistsByToken(string token)
    {
        try
        {
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");
            var response = await Client.GetUserByTokenAsync(new GetUserByTokenRequest(), headers);
            return true;
        }
        catch (Exception e) { }
        return false;
    }

    public async Task<User> GetPostUserDataById(long id, string token)
    {
        try
        {
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");
            var response = await Client.GetPostUserDataByIdAsync(new GetPostUserDataByIdRequest { Id = id }, headers);
            return response.User.ToUser();
        }
        catch (Exception e) { }
        return null;
    }

    public async Task<User> GetUserByToken(string token)
    {
        try
        {
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");
            var response = await Client.GetUserByTokenAsync(new GetUserByTokenRequest(), headers);
            return response.User.ToUser();
        }
        catch (Exception e) { }
        return null;
    }
}

public interface IUserClient
{
    Task<bool> UserExistsByToken(string token);
    Task<User> GetPostUserDataById(long id, string token);
    Task<User> GetUserByToken(string token);
}