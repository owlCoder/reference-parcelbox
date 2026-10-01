using ParcelBox.Application.Enums;
using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestContexts;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;
using ParcelBox.Domain.Parcels.Enums;
using ParcelBox.Domain.Parcels.Models;
using ParcelBox.Domain.Pickup.Enums;

namespace ParcelBox.Application.Tests.Services;

public sealed class ParcelStorageServiceTests
{
    private static readonly DateTimeOffset Now =
        DateTimeOffset.Parse("2026-01-01T10:00:00+00:00");

    [Fact]
    public async Task StoreAsync_UpdatesModelsAndCreatesPickupAccess()
    {
        var context = CreateContext();

        var result = await context.Service.StoreAsync(
            context.Parcel.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ParcelStatus.Stored, context.Parcel.Status);
        Assert.Equal(CompartmentStatus.Occupied, context.Compartment.Status);
        Assert.Equal(context.Parcel.Id, context.Compartment.ParcelId);
        Assert.Single(context.PickupAccesses.Items);
        Assert.Equal(PickupStatus.Active, context.PickupAccesses.Items[0].Status);
        Assert.Equal(PickupMessageStatus.Delivered, result.Value.MessageDelivery);
        Assert.Equal(1, context.UnitOfWork.SaveCalls);
        Assert.NotNull(context.MessageGateway.LastMessage);
    }

    [Fact]
    public async Task StoreAsync_DoesNotRollbackStorage_WhenMessageGatewayIsUnavailable()
    {
        var context = CreateContext();
        context.MessageGateway.Error = MessageGatewayError.Unavailable;

        var result = await context.Service.StoreAsync(
            context.Parcel.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ParcelStatus.Stored, context.Parcel.Status);
        Assert.Equal(CompartmentStatus.Occupied, context.Compartment.Status);
        Assert.Equal(PickupMessageStatus.Failed, result.Value.MessageDelivery);
        Assert.Equal(1, context.UnitOfWork.SaveCalls);
    }

    private static ParcelStorageTestContext CreateContext()
    {
        var parcels = new ParcelRepositoryFake();
        var compartments = new CompartmentRepositoryFake();
        var pickupAccesses = new PickupAccessRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var lockerController = new LockerControllerFake();
        var messageGateway = new MessageGatewayFake();
        var pickupCodes = new PickupCodeServiceFake();

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
        var pickupAccessService = new PickupAccessService(pickupAccesses, pickupCodes);
        var notificationService = new NotificationService(messageGateway);
        var service = new ParcelStorageService(
            parcels,
            lockerService,
            pickupAccessService,
            notificationService,
            unitOfWork,
            new FixedTimeProvider(Now));

        return new ParcelStorageTestContext(
            service,
            parcel,
            compartment,
            pickupAccesses,
            unitOfWork,
            messageGateway);
    }
}
