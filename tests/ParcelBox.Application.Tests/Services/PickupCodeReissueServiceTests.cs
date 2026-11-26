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
using ParcelBox.Domain.Pickup.Models;

namespace ParcelBox.Application.Tests.Services;

public sealed class PickupCodeReissueServiceTests
{
    private static readonly DateTimeOffset Now =
        DateTimeOffset.Parse("2026-01-01T10:00:00+00:00");

    [Fact]
    public async Task ReissueAsync_UnlocksAccessAndSendsNewCode_WhenAccessIsLocked()
    {
        var context = CreateContext();
        context.Access.Status = PickupStatus.Locked;
        context.Access.FailedAttempts = 3;
        context.PickupCodes.Code = "654321";

        var result = await context.Service.ReissueAsync(
            context.Parcel.TrackingCode,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PickupMessageStatus.Delivered, result.Value.MessageDelivery);
        Assert.Equal(PickupStatus.Active, context.Access.Status);
        Assert.Equal(0, context.Access.FailedAttempts);
        Assert.Equal("hash:654321", context.Access.CodeHash);
        Assert.Equal(Now.AddHours(24), context.Access.ExpiresAt);
        Assert.Equal(1, context.UnitOfWork.SaveCalls);
        Assert.NotNull(context.MessageGateway.LastMessage);
        Assert.Contains("654321", context.MessageGateway.LastMessage!.Text);
    }

    [Fact]
    public async Task ReissueAsync_AllowsPickupWithNewCode_AfterAccessWasLocked()
    {
        var context = CreateContext();
        context.PickupCodes.Code = "654321";

        for (var attempt = 0; attempt < 3; attempt++)
        {
            await context.PickupService.CompletePickupAsync(context.Parcel.TrackingCode, "000000", CancellationToken.None);
        }

        var locked = await context.PickupService.CompletePickupAsync(context.Parcel.TrackingCode, "123456", CancellationToken.None);
        Assert.Equal(PickupOperationError.PickupCodeLocked, locked.Error);

        var reissue = await context.Service.ReissueAsync(context.Parcel.TrackingCode, CancellationToken.None);
        Assert.True(reissue.IsSuccess);

        var oldCode = await context.PickupService.CompletePickupAsync(context.Parcel.TrackingCode, "123456", CancellationToken.None);
        Assert.Equal(PickupOperationError.PickupCodeInvalid, oldCode.Error);

        var pickup = await context.PickupService.CompletePickupAsync(context.Parcel.TrackingCode, "654321", CancellationToken.None);

        Assert.True(pickup.IsSuccess);
        Assert.Equal(ParcelStatus.PickedUp, context.Parcel.Status);
        Assert.Equal(PickupStatus.Used, context.Access.Status);
        Assert.Equal(CompartmentStatus.Available, context.Compartment.Status);
    }

    [Fact]
    public async Task ReissueAsync_RenewsExpiredAccess()
    {
        var context = CreateContext();
        context.Access.ExpiresAt = Now.AddMinutes(-5);

        var result = await context.Service.ReissueAsync(
            context.Parcel.TrackingCode,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now.AddHours(24), context.Access.ExpiresAt);
        Assert.Equal(PickupStatus.Active, context.Access.Status);
    }

    [Fact]
    public async Task ReissueAsync_Fails_WhenParcelWasAlreadyPickedUp()
    {
        var context = CreateContext();
        context.Parcel.Status = ParcelStatus.PickedUp;
        context.Access.Status = PickupStatus.Used;
        var originalHash = context.Access.CodeHash;

        var result = await context.Service.ReissueAsync(
            context.Parcel.TrackingCode,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PickupOperationError.ParcelNotStored, result.Error);
        Assert.Equal(originalHash, context.Access.CodeHash);
        Assert.Equal(0, context.UnitOfWork.SaveCalls);
        Assert.Null(context.MessageGateway.LastMessage);
    }

    [Fact]
    public async Task ReissueAsync_Fails_WhenParcelIsNotFound()
    {
        var context = CreateContext();

        var result = await context.Service.ReissueAsync(
            "UNKNOWN-999",
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PickupOperationError.ParcelNotFound, result.Error);
        Assert.Null(context.MessageGateway.LastMessage);
    }

    [Fact]
    public async Task ReissueAsync_Fails_WhenTrackingCodeIsMissing()
    {
        var context = CreateContext();

        var result = await context.Service.ReissueAsync(
            "   ",
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PickupOperationError.TrackingCodeRequired, result.Error);
    }

    [Fact]
    public async Task ReissueAsync_DoesNotSendMessage_WhenSaveFails()
    {
        var context = CreateContext();
        context.UnitOfWork.CanSave = false;

        var result = await context.Service.ReissueAsync(
            context.Parcel.TrackingCode,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PickupOperationError.PersistenceUnavailable, result.Error);
        Assert.Null(context.MessageGateway.LastMessage);
    }

    [Fact]
    public async Task ReissueAsync_KeepsNewCode_WhenMessageGatewayIsUnavailable()
    {
        var context = CreateContext();
        context.Access.Status = PickupStatus.Locked;
        context.MessageGateway.Error = MessageGatewayError.Unavailable;
        context.PickupCodes.Code = "654321";

        var result = await context.Service.ReissueAsync(
            context.Parcel.TrackingCode,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PickupMessageStatus.Failed, result.Value.MessageDelivery);
        Assert.Equal(PickupStatus.Active, context.Access.Status);
        Assert.Equal("hash:654321", context.Access.CodeHash);
        Assert.Equal(1, context.UnitOfWork.SaveCalls);
    }

    private static PickupCodeReissueTestContext CreateContext()
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
            Status = ParcelStatus.Stored,
            LockerCode = "PB-01",
            CompartmentNumber = "B1",
            CreatedAt = Now.AddHours(-1),
            StoredAt = Now.AddMinutes(-30)
        };
        parcels.Items.Add(parcel);

        var compartment = new Compartment
        {
            Id = Guid.NewGuid(),
            LockerCode = "PB-01",
            Number = "B1",
            Size = SizeCategory.Medium,
            Status = CompartmentStatus.Occupied,
            ParcelId = parcel.Id
        };
        compartments.Items.Add(compartment);

        var access = new PickupAccess
        {
            Id = Guid.NewGuid(),
            ParcelId = parcel.Id,
            CodeHash = pickupCodes.Hash("123456"),
            ExpiresAt = Now.AddHours(1),
            Status = PickupStatus.Active
        };
        pickupAccesses.Items.Add(access);

        var pickupAccessService = new PickupAccessService(pickupAccesses, pickupCodes);
        var notificationService = new NotificationService(messageGateway);
        var lockerService = new LockerService(compartments, lockerController);

        var service = new PickupCodeReissueService(
            parcels,
            pickupAccessService,
            notificationService,
            unitOfWork,
            timeProvider);

        var pickupService = new PickupService(
            parcels,
            lockerService,
            pickupAccessService,
            unitOfWork,
            timeProvider);

        return new PickupCodeReissueTestContext(
            service,
            pickupService,
            parcel,
            compartment,
            access,
            unitOfWork,
            messageGateway,
            pickupCodes);
    }
}
