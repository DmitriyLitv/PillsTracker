using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PillsTracker.Infrastructure.Persistence;

#nullable disable

namespace PillsTracker.Infrastructure.Migrations;

[DbContext(typeof(PillsTrackerDbContext))]
partial class PillsTrackerDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        // Placeholder snapshot. Regenerate with dotnet ef migrations add ...
    }
}
