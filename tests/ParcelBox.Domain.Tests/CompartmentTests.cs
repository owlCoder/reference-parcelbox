using ParcelBox.Domain.Common;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Domain.Tests;

public sealed class CompartmentTests
{
    [Theory]
    [InlineData(CompartmentSize.Small, CompartmentSize.Small, true)]
    [InlineData(CompartmentSize.Medium, CompartmentSize.Small, true)]
    [InlineData(CompartmentSize.Large, CompartmentSize.Medium, true)]
    [InlineData(CompartmentSize.Small, CompartmentSize.Medium, false)]
    public void Compatibility_depends_on_compartment_size(
        CompartmentSize compartmentSize,
        CompartmentSize requiredSize,
        bool expected)
    {
        var compartment = Compartment.Create("PB-01", "A1", compartmentSize);

        Assert.Equal(expected, compartment.CanFit(requiredSize));
    }

    [Fact]
    public void Compartment_requires_a_valid_location()
    {
        Assert.Throws<DomainException>(() => Compartment.Create("", "A1", CompartmentSize.Small));
    }
}
