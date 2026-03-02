using PillsTracker.Domain.Entities;

namespace PillsTracker.Application.Abstractions.Repositories;

public interface IMedicationRepository
{
    Task<Medication?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Medication>> GetGlobalAndByOwnerAsync(Guid ownerUserId, CancellationToken ct);
    Task<IReadOnlyList<Medication>> GetByOwnerAsync(Guid ownerUserId, CancellationToken ct);
    Task AddAsync(Medication medication, CancellationToken ct);
    void Update(Medication medication);
    void Delete(Medication medication);
}
