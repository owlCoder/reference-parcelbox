using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Abstractions;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Infrastructure.Persistence;

internal sealed class ParcelRepository(ParcelBoxDbContext db) : IParcelRepository
{
    public Task<bool> TrackingCodeExistsAsync(string trackingCode, CancellationToken cancellationToken) =>
        db.Parcels.AnyAsync(x => x.TrackingCode == trackingCode, cancellationToken);

    public Task<Parcel?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Parcels.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Parcel?> GetByTrackingCodeAsync(string trackingCode, CancellationToken cancellationToken) =>
        db.Parcels.SingleOrDefaultAsync(x => x.TrackingCode == trackingCode, cancellationToken);

    public async Task AddAsync(Parcel parcel, CancellationToken cancellationToken) =>
        await db.Parcels.AddAsync(parcel, cancellationToken);
}
