using ParcelBox.Application.Abstractions.External;
using ParcelBox.Application.Pickup;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Tests;

public sealed class PickupParcelHandlerTests
{
    private static readonly DateTimeOffset Now =
        DateTimeOffset.Parse("2026-01-01T10:00:00+00:00");

    [Fact]
    public async Task Successful_pickup_completes_all_domain_state_changes()
    {
        var parcelResult = Parcel.Register(
            "PKG-1",
            "+38160000000",
            SizeCategory.Small,
            Now);
        Assert.True(parcelResult.IsSuccess);

        var parcel = parcelResult.Value;
        Assert.True(parcel.Store("PB-01", "A1", Now).IsSuccess);

        var compartmentResult = Compartment.Create("PB-01", "A1", SizeCategory.Small);
        Assert.True(compartmentResult.IsSuccess);

        var compartment = compartmentResult.Value;
        Assert.True(compartment.Occupy(parcel.Id).IsSuccess);

        var accessResult = PickupAccess.Create(
            parcel.Id,
            "hash:123456",
            Now.AddHours(1));
        Assert.True(accessResult.IsSuccess);

        var pickupAccesses = new PickupAccessRepositoryFake();
        await pickupAccesses.AddAsync(accessResult.Value, CancellationToken.None);

        var handler = new PickupParcelHandler(
            new ParcelRepositoryFake(parcel),
            new CompartmentRepositoryFake(compartment),
            pickupAccesses,
            new LockerControllerFake(Result<LockerControllerError>.Success()),
            new PickupCodeServiceFake(),
            new DbSessionFake(),
            new FixedTimeProvider(Now));

        var result = await handler.HandleAsync("PKG-1", "123456", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ParcelStatus.PickedUp, parcel.Status);
        Assert.Equal(CompartmentStatus.Available, compartment.Status);
        Assert.Equal(PickupStatus.Used, accessResult.Value.Status);
    }

    [Fact]
    public async Task Jammed_locker_keeps_parcel_ready_for_retry()
    {
        var parcelResult = Parcel.Register(
            "PKG-1",
            "+38160000000",
            SizeCategory.Small,
            Now);
        Assert.True(parcelResult.IsSuccess);

        var parcel = parcelResult.Value;
        Assert.True(parcel.Store("PB-01", "A1", Now).IsSuccess);

        var compartmentResult = Compartment.Create("PB-01", "A1", SizeCategory.Small);
        Assert.True(compartmentResult.IsSuccess);

        var compartment = compartmentResult.Value;
        Assert.True(compartment.Occupy(parcel.Id).IsSuccess);

        var accessResult = PickupAccess.Create(
            parcel.Id,
            "hash:123456",
            Now.AddHours(1));
        Assert.True(accessResult.IsSuccess);

        var pickupAccesses = new PickupAccessRepositoryFake();
        await pickupAccesses.AddAsync(accessResult.Value, CancellationToken.None);

        var handler = new PickupParcelHandler(
            new ParcelRepositoryFake(parcel),
            new CompartmentRepositoryFake(compartment),
            pickupAccesses,
            new LockerControllerFake(Result<LockerControllerError>.Failure(LockerControllerError.Jammed)),
            new PickupCodeServiceFake(),
            new DbSessionFake(),
            new FixedTimeProvider(Now));

        var result = await handler.HandleAsync("PKG-1", "123456", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PickupOperationError.LockerJammed, result.Error);
        Assert.Equal(ParcelStatus.Stored, parcel.Status);
        Assert.Equal(CompartmentStatus.Occupied, compartment.Status);
        Assert.Equal(PickupStatus.Active, accessResult.Value.Status);
    }
}
