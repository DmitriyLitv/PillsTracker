using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PillsTracker.Application.Abstractions.Persistence;
using PillsTracker.Domain.Entities;
using PillsTracker.Infrastructure.Identity;

namespace PillsTracker.Infrastructure.Persistence;

public sealed class PillsTrackerDbContext(DbContextOptions<PillsTrackerDbContext> options)
    : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<TimeAnchor> TimeAnchors => Set<TimeAnchor>();
    public DbSet<IntakePlan> IntakePlans => Set<IntakePlan>();
    public DbSet<IntakeTimeSlot> IntakeTimeSlots => Set<IntakeTimeSlot>();
    public DbSet<ReminderEvent> ReminderEvents => Set<ReminderEvent>();
    public DbSet<IntakeLog> IntakeLogs => Set<IntakeLog>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.LastKnownTimeZoneId).HasMaxLength(128);
        });

        builder.Entity<RefreshTokenEntity>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.UserId, x.IsRevoked });
        });

        builder.Entity<Medication>(entity =>
        {
            entity.ToTable("Medications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Form).HasMaxLength(100);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.Unit).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.OwnerUserId, x.Name }).IsUnique();
        });

        builder.Entity<TimeAnchor>(entity =>
        {
            entity.ToTable("TimeAnchors");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => new { x.OwnerUserId, x.Key }).IsUnique();
        });

        builder.Entity<IntakePlan>(entity =>
        {
            entity.ToTable("IntakePlans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DoseAmount).HasPrecision(10, 3);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Medication>()
                .WithMany()
                .HasForeignKey(x => x.MedicationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<IntakeTimeSlot>(entity =>
        {
            entity.ToTable("IntakeTimeSlots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.AnchorKey).HasMaxLength(120);
            entity.Property(x => x.Instruction).HasMaxLength(500);
            entity.HasOne<IntakePlan>()
                .WithMany()
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReminderEvent>(entity =>
        {
            entity.ToTable("ReminderEvents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.ScheduledAtUtc });
            entity.HasIndex(x => new { x.PlanId, x.PlanRevision, x.ScheduledAtUtc });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<IntakePlan>()
                .WithMany()
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<IntakeLog>(entity =>
        {
            entity.ToTable("IntakeLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(1000);
            entity.HasIndex(x => new { x.UserId, x.ActionAtUtc });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ReminderEvent>()
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
