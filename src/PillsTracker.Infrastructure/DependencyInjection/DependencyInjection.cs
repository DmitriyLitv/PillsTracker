using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PillsTracker.Application.Abstractions.Context;
using PillsTracker.Application.Abstractions.Persistence;
using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Application.Abstractions.Services;
using PillsTracker.Application.Abstractions.Time;
using PillsTracker.Infrastructure.Background;
using PillsTracker.Infrastructure.Identity;
using PillsTracker.Infrastructure.Persistence;
using PillsTracker.Infrastructure.Repositories;
using PillsTracker.Infrastructure.Services;

namespace PillsTracker.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is missing.");

        services.AddDbContext<PillsTrackerDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<PillsTrackerDbContext>());

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<PillsTrackerDbContext>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserAccessor>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ITimeZoneResolver, TimeZoneResolver>();

        services.AddScoped<IMedicationRepository, MedicationRepository>();
        services.AddScoped<ITimeAnchorRepository, TimeAnchorRepository>();
        services.AddScoped<IIntakePlanRepository, IntakePlanRepository>();
        services.AddScoped<IReminderEventRepository, ReminderEventRepository>();
        services.AddScoped<IIntakeLogRepository, IntakeLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReminderEventGenerator, ReminderEventGenerator>();

        services.AddHostedService<DailyBackgroundService>();

        return services;
    }
}
