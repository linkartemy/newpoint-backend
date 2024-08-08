using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using NewPoint.Common.Models;
using NewPoint.PostAPI.Extensions;

namespace NewPoint.PostAPI.Clients;

public class UserClient : IUserClient
{
    public GrpcChannel Channel { get; private set; }
    private readonly GrpcUser.GrpcUserClient Client;
    public string Url { get; set; } = "http://localhost";

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
            return response != null;
        }
        catch (Exception e)
        {
        }
        return false;
    }

    public async Task<User> GetPostUserDataById(long id)
    {
        try
        {
            var response = await Client.GetPostUserDataByIdAsync(new GetPostUserDataByIdRequest { Id = id });
            if (response.Error != null)
            {
                throw new RpcException(new Status(StatusCode.Internal, response.Error));
            }
            return response.Data.Unpack<UserModel>().ToUser();
        }
        catch (Exception e)
        {
        }
        return null;
    }

    public async Task<User> GetUserByToken(string token)
    {
        var headers = new Metadata();
        headers.Add("Authorization", $"Bearer {token}");
        var response = await Client.GetUserByTokenAsync(new GetUserByTokenRequest(), headers);
        if (response.Error != null)
        {
            throw new RpcException(new Status(StatusCode.Internal, response.Error));
        }
        return response.Data.Unpack<UserModel>().ToUser();
    }
}

public interface IUserClient
{
    Task<bool> UserExistsByToken(string token);
    Task<User> GetPostUserDataById(long id);
    Task<User> GetUserByToken(string token);
}