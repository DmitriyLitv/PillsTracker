using Microsoft.EntityFrameworkCore;
using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Domain.Entities;
using PillsTracker.Infrastructure.Persistence;

namespace PillsTracker.Infrastructure.Repositories;

public sealed class TimeAnchorRepository(PillsTrackerDbContext dbContext) : ITimeAnchorRepository
{
    public Task<TimeAnchor?> GetByIdAsync(Guid id, CancellationToken ct)
        => dbContext.TimeAnchors.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<TimeAnchor?> GetByKeyAndOwnerAsync(string key, Guid ownerUserId, CancellationToken ct)
        => dbContext.TimeAnchors.FirstOrDefaultAsync(x => x.OwnerUserId == ownerUserId && x.Key == key, ct);

    public Task<IReadOnlyList<TimeAnchor>> GetSystemAndByOwnerAsync(Guid ownerUserId, CancellationToken ct)
        => dbContext.TimeAnchors
            .Where(x => x.OwnerUserId == null || x.OwnerUserId == ownerUserId)
            .OrderBy(x => x.Key)
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<TimeAnchor>>(t => t.Result, ct);

    public Task AddAsync(TimeAnchor anchor, CancellationToken ct)
        => dbContext.TimeAnchors.AddAsync(anchor, ct).AsTask();

    public void Update(TimeAnchor anchor) => dbContext.TimeAnchors.Update(anchor);

    public void Delete(TimeAnchor anchor) => dbContext.TimeAnchors.Remove(anchor);
}
