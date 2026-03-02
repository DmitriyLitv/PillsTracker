using Microsoft.Extensions.DependencyInjection;
using PillsTracker.Application.Abstractions.Messaging;

namespace PillsTracker.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();
        return services;
    }
}

internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    public Task<TResult> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return handler.Handle(command, cancellationToken);
    }

    public Task<TResult> Query<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return handler.Handle(query, cancellationToken);
    }
}
