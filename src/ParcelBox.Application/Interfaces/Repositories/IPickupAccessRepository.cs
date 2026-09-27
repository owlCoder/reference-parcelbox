using ParcelBox.Domain.Pickup.Models;

namespace ParcelBox.Application.Interfaces.Repositories;

public interface IPickupAccessRepository
{
    Task<PickupAccess?> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken);

    Task AddAsync(PickupAccess pickupAccess, CancellationToken cancellationToken);
}
