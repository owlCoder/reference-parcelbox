using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Abstractions;

public interface IPickupAccessRepository
{
    Task<PickupAccess?> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken);
    Task AddAsync(PickupAccess pickupAccess, CancellationToken cancellationToken);
}
