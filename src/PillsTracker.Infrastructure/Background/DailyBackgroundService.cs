using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PillsTracker.Application.Abstractions.Services;
using PillsTracker.Infrastructure.Persistence;

namespace PillsTracker.Infrastructure.Background;

public sealed class DailyBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<DailyBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromDays(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            await DelayUntilThreeAmUtc(stoppingToken);
            await RunOnce(stoppingToken);

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task RunOnce(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PillsTrackerDbContext>();
        var generator = scope.ServiceProvider.GetRequiredService<IReminderEventGenerator>();

        var users = await dbContext.IntakePlans
            .Where(x => x.Status == Domain.Enums.PlanStatus.Active)
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        foreach (var userId in users)
        {
            await generator.EnsureWindowAsync(userId, now, 7, ct);
        }

        logger.LogInformation("Daily reminder generation completed for {Count} users.", users.Count);
    }

    private static async Task DelayUntilThreeAmUtc(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var next = new DateTimeOffset(now.Year, now.Month, now.Day, 3, 0, 0, TimeSpan.Zero);
        if (next <= now)
            next = next.AddDays(1);

        await Task.Delay(next - now, ct);
    }
}
