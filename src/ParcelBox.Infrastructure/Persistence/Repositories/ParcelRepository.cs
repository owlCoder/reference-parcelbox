using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Parcels.Models;

namespace ParcelBox.Infrastructure.Persistence.Repositories;

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
            parcel => parcel.TrackingCode == trackingCode,
            cancellationToken);
    }

    public async Task<Parcel> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Parcels.SingleOrDefaultAsync(parcel => parcel.Id == id, cancellationToken) ?? new Parcel();
    }

    public async Task<Parcel> GetByTrackingCodeAsync(string trackingCode,
        CancellationToken cancellationToken)
    {
        return await _db.Parcels.SingleOrDefaultAsync(
            parcel => parcel.TrackingCode == trackingCode,
            cancellationToken) ?? new Parcel();
    }

    public async Task AddAsync(Parcel parcel, CancellationToken cancellationToken)
    {
        await _db.Parcels.AddAsync(parcel, cancellationToken);
    }
}
