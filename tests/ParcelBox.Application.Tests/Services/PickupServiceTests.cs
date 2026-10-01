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

public sealed class PickupServiceTests
{
    private static readonly DateTimeOffset Now =
        DateTimeOffset.Parse("2026-01-01T10:00:00+00:00");

    [Fact]
    public async Task CompletePickupAsync_DoesNotChangeState_WhenLockerIsJammed()
    {
        var context = CreateContext();
        context.LockerController.Error = LockerControllerError.Jammed;

        var result = await context.Service.CompletePickupAsync(
            context.Parcel.TrackingCode,
            "123456",
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PickupOperationError.LockerJammed, result.Error);
        Assert.Equal(ParcelStatus.Stored, context.Parcel.Status);
        Assert.Equal(PickupStatus.Active, context.Access.Status);
        Assert.Equal(CompartmentStatus.Occupied, context.Compartment.Status);
    }

    [Fact]
    public async Task CompletePickupAsync_LocksAccess_AfterThreeInvalidAttempts()
    {
        var context = CreateContext();

        await context.Service.CompletePickupAsync(
            context.Parcel.TrackingCode,
            "000000",
            CancellationToken.None);
        await context.Service.CompletePickupAsync(
            context.Parcel.TrackingCode,
            "000000",
            CancellationToken.None);
        var result = await context.Service.CompletePickupAsync(
            context.Parcel.TrackingCode,
            "000000",
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PickupOperationError.PickupCodeLocked, result.Error);
        Assert.Equal(PickupStatus.Locked, context.Access.Status);
        Assert.Equal(3, context.Access.FailedAttempts);
        Assert.Equal(3, context.UnitOfWork.SaveCalls);
    }

    private static PickupTestContext CreateContext()
    {
        var parcels = new ParcelRepositoryFake();
        var compartments = new CompartmentRepositoryFake();
        var pickupAccesses = new PickupAccessRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var lockerController = new LockerControllerFake();
        var pickupCodes = new PickupCodeServiceFake();

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

        var lockerService = new LockerService(compartments, lockerController);
        var pickupAccessService = new PickupAccessService(pickupAccesses, pickupCodes);
        var service = new PickupService(
            parcels,
            lockerService,
            pickupAccessService,
            unitOfWork,
            new FixedTimeProvider(Now));

        return new PickupTestContext(
            service,
            parcel,
            compartment,
            access,
            unitOfWork,
            lockerController);
    }
}
