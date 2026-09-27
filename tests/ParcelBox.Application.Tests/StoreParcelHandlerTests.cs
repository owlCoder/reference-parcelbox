using ParcelBox.Application.Abstractions.External;
using ParcelBox.Application.Parcels;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;

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
            SizeCategory.Medium,
            Now);
        var compartmentResult = Compartment.Create(
            "PB-01",
            "B1",
            SizeCategory.Medium);

        Assert.True(parcelResult.IsSuccess);
        Assert.True(compartmentResult.IsSuccess);

        var handler = new StoreParcelHandler(
            new ParcelRepositoryFake(parcelResult.Value),
            new CompartmentRepositoryFake(compartmentResult.Value),
            new PickupAccessRepositoryFake(),
            new LockerControllerFake(Result<LockerControllerError>.Success()),
            new MessageGatewayFake(Result<MessageGatewayError>.Failure(MessageGatewayError.Unavailable)),
            new PickupCodeServiceFake(),
            new DbSessionFake(),
            new FixedTimeProvider(Now));

        var result = await handler.HandleAsync(
            parcelResult.Value.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ParcelStatus.Stored, parcelResult.Value.Status);
        Assert.Equal(CompartmentStatus.Occupied, compartmentResult.Value.Status);
        Assert.Equal(PickupMessageStatus.Failed, result.Value.MessageDelivery);
    }

    [Fact]
    public async Task Jammed_locker_does_not_change_domain_state()
    {
        var parcelResult = Parcel.Register(
            "PKG-1",
            "+38160000000",
            SizeCategory.Small,
            Now);
        var compartmentResult = Compartment.Create(
            "PB-01",
            "A1",
            SizeCategory.Small);

        Assert.True(parcelResult.IsSuccess);
        Assert.True(compartmentResult.IsSuccess);

        var handler = new StoreParcelHandler(
            new ParcelRepositoryFake(parcelResult.Value),
            new CompartmentRepositoryFake(compartmentResult.Value),
            new PickupAccessRepositoryFake(),
            new LockerControllerFake(Result<LockerControllerError>.Failure(LockerControllerError.Jammed)),
            new MessageGatewayFake(Result<MessageGatewayError>.Success()),
            new PickupCodeServiceFake(),
            new DbSessionFake(),
            new FixedTimeProvider(Now));

        var result = await handler.HandleAsync(
            parcelResult.Value.Id,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ParcelOperationError.LockerJammed, result.Error);
        Assert.Equal(ParcelStatus.Registered, parcelResult.Value.Status);
        Assert.Equal(CompartmentStatus.Available, compartmentResult.Value.Status);
    }
}
