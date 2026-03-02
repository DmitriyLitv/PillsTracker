using Microsoft.EntityFrameworkCore;
using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Domain.Entities;
using PillsTracker.Infrastructure.Persistence;

namespace PillsTracker.Infrastructure.Repositories;

public sealed class MedicationRepository(PillsTrackerDbContext dbContext) : IMedicationRepository
{
    public Task<Medication?> GetByIdAsync(Guid id, CancellationToken ct)
        => dbContext.Medications.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<IReadOnlyList<Medication>> GetGlobalAndByOwnerAsync(Guid ownerUserId, CancellationToken ct)
        => dbContext.Medications
            .Where(x => x.OwnerUserId == null || x.OwnerUserId == ownerUserId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<Medication>>(t => t.Result, ct);

    public Task<IReadOnlyList<Medication>> GetByOwnerAsync(Guid ownerUserId, CancellationToken ct)
        => dbContext.Medications
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<Medication>>(t => t.Result, ct);

    public Task AddAsync(Medication medication, CancellationToken ct)
        => dbContext.Medications.AddAsync(medication, ct).AsTask();

    public void Update(Medication medication) => dbContext.Medications.Update(medication);

    public void Delete(Medication medication) => dbContext.Medications.Remove(medication);
}
