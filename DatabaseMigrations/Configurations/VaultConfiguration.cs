using System;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.Commons;
using VaultSharp.V1.SecretsEngines;
using Microsoft.Extensions.Configuration;
using VaultSharp.V1.AuthMethods.Token;

namespace ProjectApi.CustomOptions
{
    public class VaultConfigurationProvider : ConfigurationProvider
    {
        public VaultOptions _config;
        private IVaultClient _client;
        public string ConnectionString { get; set; }

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
            ConnectionString = connectionString!;

            Data.Add("database:connectionString", connectionString);
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
}