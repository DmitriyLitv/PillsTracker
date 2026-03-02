using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;

namespace PillsTracker.Tests.Unit;

public sealed class SampleUnitTest
{
    [Fact]
    public void Medication_ShouldCreateWithProvidedValues()
    {
        var medication = new Medication(
            Guid.NewGuid(),
            "Vitamin D",
            DoseUnit.Tablet,
            form: "capsule",
            notes: "after meal");

        Assert.Equal("Vitamin D", medication.Name);
        Assert.Equal(DoseUnit.Tablet, medication.Unit);
        Assert.Equal("capsule", medication.Form);
        Assert.Equal("after meal", medication.Notes);
    }
}
