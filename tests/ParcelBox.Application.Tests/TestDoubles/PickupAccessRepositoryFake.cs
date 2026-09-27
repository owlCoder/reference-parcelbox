using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Pickup.Models;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class PickupAccessRepositoryFake : IPickupAccessRepository
{
    public List<PickupAccess> Items { get; } = [];

    public Task<PickupAccess?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Items.SingleOrDefault(access => access.ParcelId == parcelId));
    }

    public Task AddAsync(PickupAccess pickupAccess, CancellationToken cancellationToken)
    {
        Items.Add(pickupAccess);
        return Task.CompletedTask;
    }
}
