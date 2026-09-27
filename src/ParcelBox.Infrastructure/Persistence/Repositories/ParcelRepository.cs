using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Infrastructure.Persistence;

internal sealed class ParcelRepository : IParcelRepository
{
    private readonly ParcelBoxDbContext _db;

    public ParcelRepository(ParcelBoxDbContext db)
    {
        _db = db;
    }

    public Task<bool> TrackingCodeExistsAsync(
        string trackingCode,
        CancellationToken cancellationToken)
    {
        return _db.Parcels.AnyAsync(
            x => x.TrackingCode == trackingCode,
            cancellationToken);
    }

    public Task<Parcel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _db.Parcels.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Parcel?> GetByTrackingCodeAsync(
        string trackingCode,
        CancellationToken cancellationToken)
    {
        return _db.Parcels.SingleOrDefaultAsync(
            x => x.TrackingCode == trackingCode,
            cancellationToken);
    }

    public async Task AddAsync(Parcel parcel, CancellationToken cancellationToken)
    {
        await _db.Parcels.AddAsync(parcel, cancellationToken);
    }
}
