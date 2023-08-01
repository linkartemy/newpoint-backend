using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using NewPoint;

namespace NewPoint;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .ConfigureKestrel(options =>
                    {
                        options.Limits.MinRequestBodyDataRate = null;
                        
                        options.ListenLocalhost(5136, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                        });
                        
                        options.ListenLocalhost(7169, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1;
                        });
                    });

            });
}