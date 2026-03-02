using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PillsTracker.Application.Abstractions.Persistence;
using PillsTracker.Domain.Entities;

namespace PillsTracker.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext(options), IApplicationDbContext
{
    public DbSet<Medication> Medications => Set<Medication>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Medication>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Dosage).HasMaxLength(100).IsRequired();
        });
    }
}
