namespace PillsTracker.Application.Abstractions.Repositories;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}
