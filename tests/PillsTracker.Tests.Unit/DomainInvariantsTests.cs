using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;

namespace PillsTracker.Tests.Unit;

public sealed class DomainInvariantsTests
{
    [Fact]
    public void Medication_ShouldThrow_WhenNameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new Medication(Guid.NewGuid(), "   ", DoseUnit.Tablet));
    }

    [Fact]
    public void IntakePlan_ShouldThrow_WhenDoseAmountIsNotPositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IntakePlan(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 1, 2),
                PlanStatus.Active,
                1,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IntakeTimeSlot_ShouldValidateByKind()
    {
        Assert.Throws<ArgumentException>(() =>
            new IntakeTimeSlot(Guid.NewGuid(), Guid.NewGuid(), SlotKind.FixedTime, null, null));

        Assert.Throws<ArgumentException>(() =>
            new IntakeTimeSlot(Guid.NewGuid(), Guid.NewGuid(), SlotKind.AnchorKey, new TimeOnly(10, 30), null));
    }
}
