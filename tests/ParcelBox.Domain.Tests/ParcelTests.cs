using ParcelBox.Domain.Common;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Domain.Tests;

public sealed class ParcelTests
{
    [Fact]
    public void Registered_parcel_can_be_stored_and_picked_up()
    {
        var parcel = Parcel.Register("PKG-1", "+38160000000", ParcelSize.Medium, DateTimeOffset.UtcNow);

        parcel.Store("PB-01", "B1", DateTimeOffset.UtcNow);
        parcel.MarkPickedUp(DateTimeOffset.UtcNow);

        Assert.Equal(ParcelStatus.PickedUp, parcel.Status);
    }

    [Fact]
    public void Registered_parcel_cannot_be_picked_up()
    {
        var parcel = Parcel.Register("PKG-1", "+38160000000", ParcelSize.Small, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() => parcel.MarkPickedUp(DateTimeOffset.UtcNow));
    }
}
