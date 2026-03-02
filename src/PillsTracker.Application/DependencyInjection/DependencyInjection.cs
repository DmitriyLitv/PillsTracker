using Microsoft.Extensions.DependencyInjection;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.Abstractions.Services;
using PillsTracker.Application.Services;
using System.Reflection;

namespace PillsTracker.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<IReminderEventGenerator, ReminderEventGenerator>();

        var assembly = Assembly.GetExecutingAssembly();
        RegisterOpenGenericHandlers(services, assembly, typeof(ICommandHandler<,>));
        RegisterOpenGenericHandlers(services, assembly, typeof(IQueryHandler<,>));

        return services;
    }

    private static void RegisterOpenGenericHandlers(IServiceCollection services, Assembly assembly, Type openGenericType)
    {
        var types = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false });

        foreach (var implementation in types)
        {
            var interfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericType);

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, implementation);
            }
        }
    }
}

internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    public Task<TResult> Send<TResult>(ICommand<TResult> cmd, CancellationToken ct)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(cmd.GetType(), typeof(TResult));
        dynamic handler = serviceProvider.GetRequiredService(handlerType);
        return handler.Handle((dynamic)cmd, ct);
    }

    public Task<TResult> Query<TResult>(IQuery<TResult> q, CancellationToken ct)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(q.GetType(), typeof(TResult));
        dynamic handler = serviceProvider.GetRequiredService(handlerType);
        return handler.Handle((dynamic)q, ct);
    }
}
