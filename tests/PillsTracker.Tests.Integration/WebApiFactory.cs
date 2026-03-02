using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace PillsTracker.Tests.Integration;

public sealed class WebApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IContainer _postgres = new ContainerBuilder()
        .WithImage("postgres:16-alpine")
        .WithEnvironment("POSTGRES_DB", "pillstracker_tests")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithPortBinding(5432, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] =
                    $"Host=localhost;Port={_postgres.GetMappedPublicPort(5432)};Database=pillstracker_tests;Username=postgres;Password=postgres"
            });
        });
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public new async Task DisposeAsync() => await _postgres.DisposeAsync();
}
