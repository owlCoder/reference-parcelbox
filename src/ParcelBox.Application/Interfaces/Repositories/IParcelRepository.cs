using ParcelBox.Domain.Parcels.Models;

namespace ParcelBox.Application.Interfaces.Repositories;

public interface IParcelRepository
{
    Task<bool> TrackingCodeExistsAsync(string trackingCode, CancellationToken cancellationToken);

    Task<Parcel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Parcel?> GetByTrackingCodeAsync(string trackingCode, CancellationToken cancellationToken);

    Task AddAsync(Parcel parcel, CancellationToken cancellationToken);
}
