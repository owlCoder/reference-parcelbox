using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Abstractions;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Infrastructure.Persistence;

internal sealed class PickupAccessRepository(ParcelBoxDbContext db) : IPickupAccessRepository
{
    public Task<PickupAccess?> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken) =>
        db.PickupAccesses.SingleOrDefaultAsync(x => x.ParcelId == parcelId, cancellationToken);

    public async Task AddAsync(PickupAccess pickupAccess, CancellationToken cancellationToken) =>
        await db.PickupAccesses.AddAsync(pickupAccess, cancellationToken);
}
