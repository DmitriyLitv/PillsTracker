using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Infrastructure.Persistence;

namespace PillsTracker.Infrastructure.Repositories;

public sealed class UnitOfWork(PillsTrackerDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
