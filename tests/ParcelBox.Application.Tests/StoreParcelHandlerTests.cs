using ParcelBox.Application.Abstractions;
using ParcelBox.Application.Parcels;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Tests;

public sealed class StoreParcelHandlerTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-01-01T10:00:00+00:00");

    [Fact]
    public async Task Message_failure_does_not_rollback_successful_storage()
    {
        var parcel = Parcel.Register("PKG-1", "+38160000000", ParcelSize.Medium, Now);
        var compartment = Compartment.Create("PB-01", "B1", CompartmentSize.Medium);
        var parcels = new ParcelRepositoryFake(parcel);
        var compartments = new CompartmentRepositoryFake(compartment);
        var pickups = new PickupRepositoryFake();
        var handler = new StoreParcelHandler(
            parcels,
            compartments,
            pickups,
            new LockerControllerFake(LockerOpenResult.Opened),
            new MessageGatewayFake(false),
            new PickupCodeServiceFake(),
            new DbSessionFake(),
            new FixedTimeProvider(Now));

        var result = await handler.HandleAsync(parcel.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ParcelStatus.Stored, parcel.Status);
        Assert.Equal(CompartmentStatus.Occupied, compartment.Status);
        Assert.Equal(MessageDeliveryStatus.Failed, pickups.Item!.MessageDelivery);
    }

    [Fact]
    public async Task Jammed_locker_does_not_change_domain_state()
    {
        var parcel = Parcel.Register("PKG-1", "+38160000000", ParcelSize.Small, Now);
        var compartment = Compartment.Create("PB-01", "A1", CompartmentSize.Small);
        var handler = new StoreParcelHandler(
            new ParcelRepositoryFake(parcel),
            new CompartmentRepositoryFake(compartment),
            new PickupRepositoryFake(),
            new LockerControllerFake(LockerOpenResult.Jammed),
            new MessageGatewayFake(true),
            new PickupCodeServiceFake(),
            new DbSessionFake(),
            new FixedTimeProvider(Now));

        var result = await handler.HandleAsync(parcel.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ParcelStatus.Registered, parcel.Status);
        Assert.Equal(CompartmentStatus.Available, compartment.Status);
    }

    private sealed class ParcelRepositoryFake(Parcel parcel) : IParcelRepository
    {
        public Task<bool> TrackingCodeExistsAsync(string trackingCode, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<Parcel?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<Parcel?>(parcel.Id == id ? parcel : null);
        public Task<Parcel?> GetByTrackingCodeAsync(string trackingCode, CancellationToken cancellationToken) => Task.FromResult<Parcel?>(parcel.TrackingCode == trackingCode ? parcel : null);
        public Task AddAsync(Parcel item, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class CompartmentRepositoryFake(Compartment compartment) : ICompartmentRepository
    {
        public Task<Compartment?> FindAvailableAsync(CompartmentSize requiredSize, CancellationToken cancellationToken) =>
            Task.FromResult<Compartment?>(compartment.CanFit(requiredSize) ? compartment : null);

        public Task<Compartment?> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken) =>
            Task.FromResult<Compartment?>(compartment.ParcelId == parcelId ? compartment : null);

        public Task<IReadOnlyList<Compartment>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Compartment>>([compartment]);
    }

    private sealed class PickupRepositoryFake : IPickupAccessRepository
    {
        public PickupAccess? Item { get; private set; }

        public Task<PickupAccess?> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken) =>
            Task.FromResult(Item);

        public Task AddAsync(PickupAccess pickupAccess, CancellationToken cancellationToken)
        {
            Item = pickupAccess;
            return Task.CompletedTask;
        }
    }

    private sealed class LockerControllerFake(LockerOpenResult result) : ILockerController
    {
        public Task<LockerOpenResult> OpenAsync(
            string lockerCode,
            string compartmentNumber,
            CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class MessageGatewayFake(bool result) : IMessageGateway
    {
        public Task<bool> SendPickupCodeAsync(
            string destination,
            string trackingCode,
            string pickupCode,
            CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class PickupCodeServiceFake : IPickupCodeService
    {
        public string Generate() => "123456";
        public string Hash(string code) => $"hash:{code}";
    }

    private sealed class DbSessionFake : IAppDbSession
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
