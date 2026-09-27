using ParcelBox.Application.DTOs.Parcels;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Tests;

public sealed class ParcelServiceTests
{
    private static readonly DateTimeOffset Now =
        DateTimeOffset.Parse("2026-01-01T10:00:00+00:00");

    [Fact]
    public async Task StoreAsync_UpdatesModelsAndCreatesPickupAccess()
    {
        var parcels = new ParcelRepositoryFake();
        var compartments = new CompartmentRepositoryFake();
        var pickupAccesses = new PickupAccessRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var lockerController = new LockerControllerFake();
        var messageGateway = new MessageGatewayFake();
        var pickupCodes = new PickupCodeServiceFake();
        var timeProvider = new FixedTimeProvider(Now);

        var parcel = new Parcel
        {
            Id = Guid.NewGuid(),
            TrackingCode = "PKG-001",
            RecipientPhone = "+38160111222",
            Size = SizeCategory.Medium,
            Status = ParcelStatus.Registered,
            CreatedAt = Now
        };
        parcels.Items.Add(parcel);

        var compartment = new Compartment
        {
            Id = Guid.NewGuid(),
            LockerCode = "PB-01",
            Number = "B1",
            Size = SizeCategory.Medium,
            Status = CompartmentStatus.Available
        };
        compartments.Items.Add(compartment);

        var lockerService = new LockerService(compartments, lockerController);
        var pickupService = new PickupService(
            parcels,
            pickupAccesses,
            lockerService,
            messageGateway,
            pickupCodes,
            unitOfWork,
            timeProvider);
        var parcelService = new ParcelService(
            parcels,
            lockerService,
            pickupService,
            unitOfWork,
            timeProvider);

        var result = await parcelService.StoreAsync(parcel.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ParcelStatus.Stored, parcel.Status);
        Assert.Equal(CompartmentStatus.Occupied, compartment.Status);
        Assert.Equal(parcel.Id, compartment.ParcelId);
        Assert.Single(pickupAccesses.Items);
        Assert.Equal(PickupStatus.Active, pickupAccesses.Items[0].Status);
        Assert.Equal(PickupMessageStatus.Delivered, result.Value.MessageDelivery);
        Assert.Equal(1, unitOfWork.SaveCalls);
        Assert.NotNull(messageGateway.LastMessage);
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateTrackingCode()
    {
        var parcels = new ParcelRepositoryFake();
        parcels.Items.Add(new Parcel
        {
            Id = Guid.NewGuid(),
            TrackingCode = "PKG-001",
            RecipientPhone = "+38160111222",
            Size = SizeCategory.Small,
            Status = ParcelStatus.Registered,
            CreatedAt = Now
        });

        var compartments = new CompartmentRepositoryFake();
        var lockerService = new LockerService(compartments, new LockerControllerFake());
        var unitOfWork = new UnitOfWorkFake();
        var pickupService = new PickupService(
            parcels,
            new PickupAccessRepositoryFake(),
            lockerService,
            new MessageGatewayFake(),
            new PickupCodeServiceFake(),
            unitOfWork,
            new FixedTimeProvider(Now));
        var service = new ParcelService(
            parcels,
            lockerService,
            pickupService,
            unitOfWork,
            new FixedTimeProvider(Now));

        var result = await service.RegisterAsync(
            new RegisterParcelInput("PKG-001", "+38160111333", SizeCategory.Medium),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ParcelOperationError.TrackingCodeAlreadyExists, result.Error);
    }
}
