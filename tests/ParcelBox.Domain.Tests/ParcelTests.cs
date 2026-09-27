using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Domain.Tests;

public sealed class ParcelTests
{
    [Fact]
    public void Registered_parcel_can_be_stored_and_picked_up()
    {
        var now = DateTimeOffset.Parse("2026-01-01T10:00:00+00:00");
        var registerResult = Parcel.Register(
            "PKG-1",
            "+38160000000",
            SizeCategory.Medium,
            now);

        Assert.True(registerResult.IsSuccess);

        var parcel = registerResult.Value;
        var storeResult = parcel.Store("PB-01", "B1", now.AddMinutes(1));
        var pickupResult = parcel.MarkPickedUp(now.AddMinutes(2));

        Assert.True(storeResult.IsSuccess);
        Assert.True(pickupResult.IsSuccess);
        Assert.Equal(ParcelStatus.PickedUp, parcel.Status);
    }

    [Fact]
    public void Registered_parcel_cannot_be_picked_up()
    {
        var registerResult = Parcel.Register(
            "PKG-1",
            "+38160000000",
            SizeCategory.Small,
            DateTimeOffset.UtcNow);

        Assert.True(registerResult.IsSuccess);

        var result = registerResult.Value.MarkPickedUp(DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ParcelError.NotStoredForPickup, result.Error);
    }

    [Fact]
    public void Parcel_rejects_undefined_size()
    {
        var result = Parcel.Register(
            "PKG-1",
            "+38160000000",
            (SizeCategory)999,
            DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ParcelError.InvalidSize, result.Error);
    }
}
