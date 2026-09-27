using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Domain.Tests;

public sealed class CompartmentTests
{
    [Theory]
    [InlineData(SizeCategory.Small, SizeCategory.Small, true)]
    [InlineData(SizeCategory.Medium, SizeCategory.Small, true)]
    [InlineData(SizeCategory.Large, SizeCategory.Medium, true)]
    [InlineData(SizeCategory.Small, SizeCategory.Medium, false)]
    public void Compatibility_depends_on_size(
        SizeCategory compartmentSize,
        SizeCategory requiredSize,
        bool expected)
    {
        var createResult = Compartment.Create("PB-01", "A1", compartmentSize);

        Assert.True(createResult.IsSuccess);
        Assert.Equal(expected, createResult.Value.CanFit(requiredSize));
    }

    [Fact]
    public void Compartment_requires_a_valid_location()
    {
        var result = Compartment.Create("", "A1", SizeCategory.Small);

        Assert.True(result.IsFailure);
        Assert.Equal(CompartmentError.LockerCodeRequired, result.Error);
    }
}
