using PillsTracker.Domain.Entities;

namespace PillsTracker.Application.Abstractions.Repositories;

public interface ITimeAnchorRepository
{
    Task<TimeAnchor?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<TimeAnchor?> GetByKeyAndOwnerAsync(string key, Guid ownerUserId, CancellationToken ct);
    Task<IReadOnlyList<TimeAnchor>> GetSystemAndByOwnerAsync(Guid ownerUserId, CancellationToken ct);
    Task AddAsync(TimeAnchor anchor, CancellationToken ct);
    void Update(TimeAnchor anchor);
    void Delete(TimeAnchor anchor);
}
