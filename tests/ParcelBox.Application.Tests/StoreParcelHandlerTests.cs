using ParcelBox.Application.Abstractions.External;
using ParcelBox.Application.Parcels;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Tests;

public sealed class StoreParcelHandlerTests
{
    private static readonly DateTimeOffset Now =
        DateTimeOffset.Parse("2026-01-01T10:00:00+00:00");

    [Fact]
    public async Task Message_failure_does_not_rollback_successful_storage()
    {
        var parcelResult = Parcel.Register(
            "PKG-1",
            "+38160000000",
            ParcelSize.Medium,
            Now);
        var compartmentResult = Compartment.Create(
            "PB-01",
            "B1",
            CompartmentSize.Medium);

        Assert.True(parcelResult.IsSuccess);
        Assert.True(compartmentResult.IsSuccess);

        var parcels = new ParcelRepositoryFake(parcelResult.Value);
        var compartments = new CompartmentRepositoryFake(compartmentResult.Value);
        var pickups = new PickupAccessRepositoryFake();
        var handler = new StoreParcelHandler(
            parcels,
            compartments,
            pickups,
            new LockerControllerFake(Result.Success()),
            new MessageGatewayFake(Result.Failure(MessageGatewayErrors.Unavailable)),
            new PickupCodeServiceFake(),
            new DbSessionFake(),
            new FixedTimeProvider(Now));

        var result = await handler.HandleAsync(
            parcelResult.Value.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ParcelStatus.Stored, parcelResult.Value.Status);
        Assert.Equal(CompartmentStatus.Occupied, compartmentResult.Value.Status);
        Assert.Equal(MessageDeliveryStatus.Failed, pickups.Item!.MessageDelivery);
    }

    [Fact]
    public async Task Jammed_locker_does_not_change_domain_state()
    {
        var parcelResult = Parcel.Register(
            "PKG-1",
            "+38160000000",
            ParcelSize.Small,
            Now);
        var compartmentResult = Compartment.Create(
            "PB-01",
            "A1",
            CompartmentSize.Small);

        Assert.True(parcelResult.IsSuccess);
        Assert.True(compartmentResult.IsSuccess);

        var handler = new StoreParcelHandler(
            new ParcelRepositoryFake(parcelResult.Value),
            new CompartmentRepositoryFake(compartmentResult.Value),
            new PickupAccessRepositoryFake(),
            new LockerControllerFake(Result.Failure(LockerControllerErrors.Jammed)),
            new MessageGatewayFake(Result.Success()),
            new PickupCodeServiceFake(),
            new DbSessionFake(),
            new FixedTimeProvider(Now));

        var result = await handler.HandleAsync(
            parcelResult.Value.Id,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LockerControllerErrors.Jammed, result.Error);
        Assert.Equal(ParcelStatus.Registered, parcelResult.Value.Status);
        Assert.Equal(CompartmentStatus.Available, compartmentResult.Value.Status);
    }
}
