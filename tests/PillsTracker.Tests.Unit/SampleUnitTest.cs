using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;

namespace PillsTracker.Tests.Unit;

public sealed class SampleUnitTest
{
    [Fact]
    public void Medication_ShouldCreateWithProvidedValues()
    {
        var medication = new Medication(Guid.NewGuid(), "Vitamin D", DoseUnit.Tablet, notes: "1000 IU");

        Assert.Equal("Vitamin D", medication.Name);
        Assert.Equal(DoseUnit.Tablet, medication.Unit);
    }
}
