using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PillsTracker.Application.Abstractions.Persistence;
using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;

namespace PillsTracker.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext(options), IApplicationDbContext
{
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<TimeAnchor> TimeAnchors => Set<TimeAnchor>();
    public DbSet<IntakePlan> IntakePlans => Set<IntakePlan>();
    public DbSet<IntakeTimeSlot> IntakeTimeSlots => Set<IntakeTimeSlot>();
    public DbSet<ReminderEvent> ReminderEvents => Set<ReminderEvent>();
    public DbSet<IntakeLog> IntakeLogs => Set<IntakeLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Medication>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Form).HasMaxLength(100);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.Unit).HasConversion<string>().IsRequired();
        });

        builder.Entity<TimeAnchor>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(120).IsRequired();
        });

        builder.Entity<IntakePlan>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DoseAmount).HasPrecision(10, 3);
            entity.Property(x => x.Status).HasConversion<string>().IsRequired();
        });

        builder.Entity<IntakeTimeSlot>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Kind).HasConversion<string>().IsRequired();
            entity.Property(x => x.AnchorKey).HasMaxLength(120);
            entity.Property(x => x.Instruction).HasMaxLength(500);
        });

        builder.Entity<ReminderEvent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>().IsRequired();
        });

        builder.Entity<IntakeLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasConversion<string>().IsRequired();
            entity.Property(x => x.Note).HasMaxLength(1000);
            entity.Property(x => x.Dosage).HasMaxLength(100).IsRequired();
        });
    }
}
