using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;
using NewPoint.Common.Configurations;
using NewPoint.FeedAPI.Services;
using NewPoint.FeedAPI.Middleware;
using NewPoint.FeedAPI.Clients;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Filters;

namespace NewPoint.FeedAPI;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private bool InDocker
        => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        services.AddGrpc(options =>
    {
        options.Interceptors.Add<AuthorizationInterceptor>();
    });

        services.AddSingleton<AuthorizationInterceptor>();
        services.AddMvc(options => options.EnableEndpointRouting = false);

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(origin => true)
                        .AllowCredentials();
                });
        });

        services.AddTransient<IUserClient, UserClient>();
        services.AddTransient<IArticleClient, ArticleClient>();
        services.AddTransient<IPostClient, PostClient>();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API",
                Version = "v1",
                Description = "API"
            });
        });

        services.AddControllers().AddNewtonsoftJson(options =>
            options.SerializerSettings.Converters.Add(new StringEnumConverter()));
        services.AddControllers().AddNewtonsoftJson();

        services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = long.MaxValue;
            x.MultipartHeadersLengthLimit = int.MaxValue;
        });

        services.AddMvc();

        services.Configure<VaultOptions>(Configuration.GetSection("Vault"));

        var vaultOptions = Configuration.GetSection("Vault").Get<VaultOptions>();
        var vaultService = new VaultConfigurationProvider(vaultOptions);
        vaultService.Load();

        services.AddSwaggerGenNewtonsoftSupport();
        services.AddGrpcReflection();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() || env.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = "swagger";
            });
            app.UseDeveloperExceptionPage();
        }

        // app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors(options
            => options
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(origin => true)
                .AllowCredentials());

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<FeedService>();
            endpoints.MapGrpcReflectionService();
            endpoints.MapControllers();
        });
    }
}