using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Domain.Entities;
using PillsTracker.Infrastructure.Persistence;

namespace PillsTracker.Infrastructure.Repositories;

public sealed class IntakeLogRepository(PillsTrackerDbContext dbContext) : IIntakeLogRepository
{
    public Task AddAsync(IntakeLog intakeLog, CancellationToken ct)
        => dbContext.IntakeLogs.AddAsync(intakeLog, ct).AsTask();
}
