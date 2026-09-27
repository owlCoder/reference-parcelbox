using ParcelBox.Domain.Common;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Domain.Tests;

public sealed class ParcelTests
{
    [Fact]
    public void Registered_parcel_can_be_stored_and_picked_up()
    {
        var now = DateTimeOffset.Parse("2026-01-01T10:00:00+00:00");
        var parcel = Parcel.Register("PKG-1", "+38160000000", ParcelSize.Medium, now);

        parcel.Store("PB-01", "B1", now.AddMinutes(1));
        parcel.MarkPickedUp(now.AddMinutes(2));

        Assert.Equal(ParcelStatus.PickedUp, parcel.Status);
    }

    [Fact]
    public void Registered_parcel_cannot_be_picked_up()
    {
        var parcel = Parcel.Register("PKG-1", "+38160000000", ParcelSize.Small, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() => parcel.MarkPickedUp(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Parcel_rejects_undefined_size()
    {
        Assert.Throws<DomainException>(() =>
            Parcel.Register("PKG-1", "+38160000000", (ParcelSize)999, DateTimeOffset.UtcNow));
    }
}
