using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PillsTracker.Contracts.Auth;
using PillsTracker.Contracts.Events;
using PillsTracker.Contracts.Medication;
using PillsTracker.Contracts.Plans;
using PillsTracker.Contracts.TimeAnchors;

namespace PillsTracker.Tests.Integration;

public sealed class ApiFlowTests(WebApiFactory factory) : IClassFixture<WebApiFactory>
{
    [Fact]
    public async Task Register_Login_And_Logout_RevokesRefreshToken()
    {
        var client = factory.CreateClient();
        var email = $"user_{Guid.NewGuid():N}@test.local";
        var password = "Test123!Aa";

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, password));
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password, "Europe/Moscow"));
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(auth.RefreshToken, null));
        Assert.True(refresh.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Medication_Anchor_Plan_Events_Flow_Works()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAndLogin(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        client.DefaultRequestHeaders.Add("X-User-TimeZone", "Europe/Moscow");

        var createMedication = await client.PostAsJsonAsync("/api/medications", new CreateMedicationRequest("Vitamin C", "Tablet", null, null));
        createMedication.EnsureSuccessStatusCode();
        var medication = await createMedication.Content.ReadFromJsonAsync<MedicationDto>();
        Assert.NotNull(medication);

        var updateMedication = await client.PutAsJsonAsync($"/api/medications/{medication!.Id}", new UpdateMedicationRequest("Vitamin C Updated", "Tablet", null, "note"));
        updateMedication.EnsureSuccessStatusCode();

        var medsList = await client.GetFromJsonAsync<List<MedicationDto>>("/api/medications");
        Assert.NotNull(medsList);
        Assert.Contains(medsList!, x => x.Id == medication.Id);

        var upsertAnchor = await client.PutAsJsonAsync("/api/anchors", new UpsertTimeAnchorRequest("Перед сном", "22:00"));
        upsertAnchor.EnsureSuccessStatusCode();

        var anchors = await client.GetFromJsonAsync<List<TimeAnchorDto>>("/api/anchors");
        Assert.NotNull(anchors);
        Assert.Contains(anchors!, a => a.Key == "Перед сном");

        var createPlan = await client.PostAsJsonAsync("/api/plans", new CreatePlanRequest(
            medication.Id,
            1,
            DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            7,
            [new CreatePlanSlot("AnchorKey", null, "Перед сном", null)]));
        createPlan.EnsureSuccessStatusCode();

        var events = await client.GetFromJsonAsync<List<ReminderEventDto>>("/api/events");
        Assert.NotNull(events);
        Assert.NotEmpty(events!);

        var takeResponse = await client.PutAsJsonAsync($"/api/events/{events![0].Id}/take", new TakeEventRequest(null));
        takeResponse.EnsureSuccessStatusCode();

        var taken = await takeResponse.Content.ReadFromJsonAsync<ReminderEventDto>();
        Assert.NotNull(taken);
        Assert.Equal("Taken", taken!.Status);

        var deleteMedication = await client.DeleteAsync($"/api/medications/{medication.Id}");
        Assert.True(deleteMedication.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.BadRequest);
    }

    private static async Task<AuthResponse> RegisterAndLogin(HttpClient client)
    {
        var email = $"user_{Guid.NewGuid():N}@test.local";
        var password = "Test123!Aa";

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, password));
        register.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password, "Europe/Moscow"));
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!;
    }
}
