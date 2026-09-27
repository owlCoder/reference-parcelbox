using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Infrastructure.Persistence;

internal sealed class PickupAccessRepository : IPickupAccessRepository
{
    private readonly ParcelBoxDbContext _db;

    public PickupAccessRepository(ParcelBoxDbContext db)
    {
        _db = db;
    }

    public Task<PickupAccess?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        return _db.PickupAccesses.SingleOrDefaultAsync(
            x => x.ParcelId == parcelId,
            cancellationToken);
    }

    public async Task AddAsync(
        PickupAccess pickupAccess,
        CancellationToken cancellationToken)
    {
        await _db.PickupAccesses.AddAsync(pickupAccess, cancellationToken);
    }
}
