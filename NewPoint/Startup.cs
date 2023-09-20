using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NewPoint.Configurations;
using NewPoint.Handlers;
using NewPoint.Repositories;
using NewPoint.Services;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Filters;

namespace NewPoint;

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
        services.AddGrpc(options => { options.EnableDetailedErrors = true; });
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

        // services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IUserRepository, UserRepository>();
        // services.AddSingleton<IPostService, PostService>();
        services.AddSingleton<IPostRepository, PostRepository>();
        // services.AddSingleton<ICodeService, CodeService>();
        services.AddSingleton<ICodeRepository, CodeRepository>();
        // services.AddSingleton<ICommentService, CommentService>();
        services.AddSingleton<ICommentRepository, CommentRepository>();
        // services.AddSingleton<ICommentService, CommentService>();
        services.AddSingleton<IImageRepository, ImageRepository>();
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

        services.Configure<IISServerOptions>(options => { options.MaxRequestBodySize = long.MaxValue; });

        services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = long.MaxValue;
            x.MultipartHeadersLengthLimit = int.MaxValue;
        });

        services.AddMvc();

        DatabaseHandler.ConnectionString = Configuration.GetConnectionString("Postgres");

        AuthenticationHandler.JwtToken = Configuration.GetSection(nameof(JwtConfiguration)).GetValue<string>("token");

        SmtpHandler.Configuration = new SmtpConfiguration(Configuration.GetSection(nameof(SmtpConfiguration)));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        Configuration.GetSection(nameof(JwtConfiguration)).GetValue<string>("Token"))),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            context.Response.Headers.Add("is-token-expired", "true");
                        return Task.CompletedTask;
                    }
                };
            });

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

        app.UseHttpsRedirection();

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
            endpoints.MapGrpcService<UserService>();
            endpoints.MapGrpcService<PostService>();
            endpoints.MapGrpcService<CommentService>();
            endpoints.MapGrpcService<CodeService>();
            endpoints.MapGrpcReflectionService();
            endpoints.MapControllers();
        });
    }
}