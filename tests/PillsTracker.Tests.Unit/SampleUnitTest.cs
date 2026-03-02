using PillsTracker.Domain.Entities;

namespace PillsTracker.Tests.Unit;

public sealed class SampleUnitTest
{
    [Fact]
    public void Medication_ShouldCreateWithProvidedValues()
    {
        var medication = new Medication
        {
            Name = "Vitamin D",
            Dosage = "1000 IU"
        };

        Assert.Equal("Vitamin D", medication.Name);
        Assert.Equal("1000 IU", medication.Dosage);
    }
}
