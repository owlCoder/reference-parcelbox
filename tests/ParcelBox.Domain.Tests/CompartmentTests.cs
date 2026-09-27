using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Domain.Tests;

public sealed class CompartmentTests
{
    [Theory]
    [InlineData(ParcelSize.Small, ParcelSize.Small, true)]
    [InlineData(ParcelSize.Medium, ParcelSize.Small, true)]
    [InlineData(ParcelSize.Large, ParcelSize.Medium, true)]
    [InlineData(ParcelSize.Small, ParcelSize.Medium, false)]
    public void Compatibility_depends_on_compartment_size(ParcelSize compartmentSize, ParcelSize parcelSize, bool expected)
    {
        var compartment = new Compartment(Guid.NewGuid(), "PB-01", "A1", compartmentSize);

        Assert.Equal(expected, compartment.CanFit(parcelSize));
    }
}
