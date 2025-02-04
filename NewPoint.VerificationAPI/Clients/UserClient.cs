using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace NewPoint.VerificationAPI.Clients;

public class UserClient : IUserClient
{
    public GrpcChannel Channel { get; private set; }
    private readonly GrpcUser.GrpcUserClient Client;
    public string Url { get; set; } = "http://newpoint-userapi:5137";

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
}

public interface IUserClient
{
    Task<bool> UserExistsByToken(string token);
}