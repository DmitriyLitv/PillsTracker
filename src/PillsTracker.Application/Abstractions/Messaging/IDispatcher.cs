namespace PillsTracker.Application.Abstractions.Messaging;

public interface IDispatcher
{
    Task<TResult> Send<TResult>(ICommand<TResult> cmd, CancellationToken ct);
    Task<TResult> Query<TResult>(IQuery<TResult> q, CancellationToken ct);
}
