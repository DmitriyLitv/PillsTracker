using PillsTracker.Domain.Entities;

namespace PillsTracker.Application.Abstractions.Repositories;

public interface IIntakeLogRepository
{
    Task AddAsync(IntakeLog intakeLog, CancellationToken ct);
}
