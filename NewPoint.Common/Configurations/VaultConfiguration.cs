using System;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.Commons;
using VaultSharp.V1.SecretsEngines;
using Microsoft.Extensions.Configuration;
using NewPoint.Common.Configurations;
using NewPoint.Common.Handlers;
using VaultSharp.V1.AuthMethods.Token;

namespace NewPoint.Common.Configurations;

public class VaultConfigurationProvider : ConfigurationProvider
{
    public VaultOptions _config;
    private IVaultClient _client;

    public VaultConfigurationProvider(VaultOptions config)
    {
        _config = config;

        var vaultClientSettings = new VaultClientSettings(
            _config.Address,
            new TokenAuthMethodInfo(_config.Token)
        );

        _client = new VaultClient(vaultClientSettings);
    }

    public override void Load()
    {
        LoadAsync().Wait();
    }

    public async Task LoadAsync()
    {
        await GetDatabaseCredentialsAsync();
        await GetSmtpCredentials();
        await GetS3Credentials();
        await GetJwtToken();
        await GetRedisCredentials();
    }

    public async Task<Secret<SecretData>> GetSecretAsync(string path)
    {
        Secret<SecretData> secrets = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            path, mountPoint: "kv");

        return secrets;
    }

    public async Task GetDatabaseCredentialsAsync()
    {
        var connectionString = "";

        if (_config.SecretType == "kv")
        {
            var secrets = await GetSecretAsync("ConnectionStrings");

            if (secrets?.Data?.Data != null && secrets.Data.Data.TryGetValue("Postgres", out object secretValue))
            {
                connectionString = secretValue.ToString();
            }
        }
        DatabaseHandler.ConnectionString = connectionString!;

        Data.Add("database:connectionString", connectionString);
    }

    public async Task GetSmtpCredentials()
    {
        var smtpConfig = new SmtpConfiguration();

        if (_config.SecretType == "kv")
        {
            var secrets = await GetSecretAsync("SMTP");

            if (secrets?.Data?.Data != null)
            {
                smtpConfig.Email = secrets.Data.Data["Email"].ToString()!;
                smtpConfig.Server = secrets.Data.Data["Server"].ToString()!;
                smtpConfig.Password = secrets.Data.Data["Password"].ToString()!;
                smtpConfig.Port = int.Parse(secrets.Data.Data["Port"].ToString()!);
            }
        }

        SmtpHandler.Configuration = smtpConfig;

        Data.Add("smtp:email", smtpConfig.Email);
        Data.Add("smtp:server", smtpConfig.Server);
        Data.Add("smtp:password", smtpConfig.Password);
        Data.Add("smtp:port", smtpConfig.Port.ToString());
    }

    public async Task GetS3Credentials()
    {
        var s3Config = new S3Configuration();

        if (_config.SecretType == "kv")
        {
            var secrets = await GetSecretAsync("Minio");

            if (secrets?.Data?.Data != null)
            {
                s3Config.Endpoint = secrets.Data.Data["Endpoint"].ToString()!;
                s3Config.AccessKey = secrets.Data.Data["AccessKey"].ToString()!;
                s3Config.SecretKey = secrets.Data.Data["SecretKey"].ToString()!;
                s3Config.UserImagesBucket = secrets.Data.Data["UserImagesBucket"].ToString()!;
            }

        }

        S3Handler.Configuration = s3Config;

        Data.Add("s3:endpoint", s3Config.Endpoint);
        Data.Add("s3:accessKey", s3Config.AccessKey);
        Data.Add("s3:secretKey", s3Config.SecretKey);
        Data.Add("s3:userImagesBucket", s3Config.UserImagesBucket);
    }

    public async Task GetJwtToken()
    {
        var jwtConfig = new JwtConfiguration();

        if (_config.SecretType == "kv")
        {
            var secrets = await GetSecretAsync("JwtConfiguration");

            if (secrets?.Data?.Data != null)
            {
                jwtConfig.Token = secrets.Data.Data["Token"].ToString()!;
            }

        }

        AuthenticationHandler.JwtToken = jwtConfig.Token;

        Data.Add("jwt:token", jwtConfig.Token);
    }

    public async Task GetRedisCredentials()
    {
        var connectionString = "";

        if (_config.SecretType == "kv")
        {
            var secrets = await GetSecretAsync("ConnectionStrings");

            if (secrets?.Data?.Data != null && secrets.Data.Data.TryGetValue("Redis", out object secretValue))
            {
                connectionString = secretValue.ToString();
            }
        }
        RedisHandler.ConnectionString = connectionString!;

        Data.Add("redis:connectionString", connectionString);
    }
}

public class VaultConfigurationSource : IConfigurationSource
{
    private VaultOptions _config;

    public VaultConfigurationSource(Action<VaultOptions> config)
    {
        _config = new VaultOptions();
        config.Invoke(_config);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new VaultConfigurationProvider(_config);
    }
}

public class VaultOptions
{
    public string Address { get; set; }
    public string Role { get; set; }
    public string Secret { get; set; }
    public string MountPath { get; set; }
    public string SecretType { get; set; }
    public string Token { get; set; }
}

public static class VaultExtensions
{
    public static IConfigurationBuilder AddVault(this IConfigurationBuilder configuration,
    Action<VaultOptions> options)
    {
        var vaultOptions = new VaultConfigurationSource(options);
        configuration.Add(vaultOptions);
        return configuration;
    }
}