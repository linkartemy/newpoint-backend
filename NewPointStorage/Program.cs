using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace NewPointStorage;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .ConfigureKestrel(options =>
                    {
                        options.Limits.MinRequestBodyDataRate = null;

                        options.ListenLocalhost(5138,
                            listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });

                        options.ListenLocalhost(7171,
                            listenOptions => { listenOptions.Protocols = HttpProtocols.Http1; });
                    });
            });
    }
}