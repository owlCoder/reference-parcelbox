using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Pickup.Models;

namespace ParcelBox.Infrastructure.Persistence.Repositories;

internal sealed class PickupAccessRepository : IPickupAccessRepository
{
    private readonly ParcelBoxDbContext _db;

    public PickupAccessRepository(ParcelBoxDbContext db)
    {
        _db = db;
    }

    public async Task<PickupAccess> GetByParcelIdAsync(Guid parcelId,
        CancellationToken cancellationToken)
    {
        return await _db.PickupAccesses.SingleOrDefaultAsync(
            access => access.ParcelId == parcelId,
            cancellationToken) ?? new PickupAccess();
    }

    public async Task AddAsync(PickupAccess pickupAccess, CancellationToken cancellationToken)
    {
        await _db.PickupAccesses.AddAsync(pickupAccess, cancellationToken);
    }
}
