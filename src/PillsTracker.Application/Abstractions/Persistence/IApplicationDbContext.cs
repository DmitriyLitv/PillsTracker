using Microsoft.EntityFrameworkCore;
using PillsTracker.Domain.Entities;

namespace PillsTracker.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Medication> Medications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
