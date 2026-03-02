namespace PillsTracker.Tests.Integration;

public sealed class HealthEndpointTests : IClassFixture<WebApiFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }
}
