using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ProjectApi.CustomOptions;

namespace DatabaseMigrations;

class Program
{
    private static IConfigurationRoot? _configuration;

    static void Main(string[] args)
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(Environment.ProcessPath).Parent.Parent.Parent.FullName)
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var vaultOptions = _configuration.GetSection("Vault").Get<VaultOptions>();
        var vaultService = new VaultConfigurationProvider(vaultOptions);
        vaultService.Load();
        var connectionString = vaultService.ConnectionString;

        using var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(configuration =>
                configuration
                    .AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(Program).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);

        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateDown(0);
        runner.MigrateUp();
    }
}

