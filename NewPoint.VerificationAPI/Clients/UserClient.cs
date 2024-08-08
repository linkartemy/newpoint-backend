using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using NewPoint;

public class UserClient
{
    public GrpcChannel Channel { get; private set; }
    private readonly GrpcUser.GrpcUserClient Client;

    public UserClient(string url)
    {
        Channel = GrpcChannel.ForAddress(url);
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
