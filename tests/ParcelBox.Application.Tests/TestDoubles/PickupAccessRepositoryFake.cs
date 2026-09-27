using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class PickupAccessRepositoryFake : IPickupAccessRepository
{
    public PickupAccess? Item { get; private set; }

    public Task<PickupAccess?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Item);
    }

    public Task AddAsync(PickupAccess pickupAccess, CancellationToken cancellationToken)
    {
        Item = pickupAccess;
        return Task.CompletedTask;
    }
}
