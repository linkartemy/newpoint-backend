using Microsoft.AspNetCore.Server.Kestrel.Core;
using NewPoint.Common.Configurations;

namespace NewPoint.BookmarkAPI;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables(prefix: "VAULT_");

                var builtConfig = config.Build();

                if (builtConfig.GetSection("Vault")["Role"] != null)
                {
                    config.AddVault(options =>
                    {
                        var vaultOptions = builtConfig.GetSection("Vault");
                        options.Address = vaultOptions["Address"];
                        options.Role = vaultOptions["Role"];
                        options.MountPath = vaultOptions["MountPath"];
                        options.SecretType = vaultOptions["SecretType"];
                        options.Secret = vaultOptions["Secret"];
                        options.Token = vaultOptions["Token"];
                    });
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .ConfigureKestrel(options =>
                    {
                        options.Limits.MinRequestBodyDataRate = null;
                        options.Limits.MaxRequestBodySize = long.MaxValue;

                        options.ListenAnyIP(5144,
                            listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                    });
            });
    }
}