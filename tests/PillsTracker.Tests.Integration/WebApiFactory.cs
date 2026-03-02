using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PillsTracker.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PillsTracker.Tests.Integration;

public sealed class WebApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("pillstracker_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["Jwt:Issuer"] = "PillsTrackerTests",
                ["Jwt:Audience"] = "PillsTrackerTestsClients",
                ["Jwt:Key"] = "test-super-secret-key-test-super-secret-key",
                ["Jwt:AccessTokenMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PillsTrackerDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public new async Task DisposeAsync() => await _postgres.DisposeAsync();
}
