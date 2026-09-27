using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Abstractions;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

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

internal sealed class CompartmentRepository(ParcelBoxDbContext db) : ICompartmentRepository
{
    public Task<Compartment?> FindAvailableAsync(ParcelSize parcelSize, CancellationToken cancellationToken) =>
        db.Compartments
            .Where(x => x.Status == CompartmentStatus.Available && (int)x.Size >= (int)parcelSize)
            .OrderBy(x => x.Size)
            .ThenBy(x => x.Number)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Compartment?> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken) =>
        db.Compartments.SingleOrDefaultAsync(x => x.ParcelId == parcelId, cancellationToken);

    public async Task<IReadOnlyList<Compartment>> ListAsync(CancellationToken cancellationToken) =>
        await db.Compartments.OrderBy(x => x.LockerCode).ThenBy(x => x.Number).ToListAsync(cancellationToken);
}

internal sealed class PickupAccessRepository(ParcelBoxDbContext db) : IPickupAccessRepository
{
    public Task<PickupAccess?> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken) =>
        db.PickupAccesses.SingleOrDefaultAsync(x => x.ParcelId == parcelId, cancellationToken);

    public async Task AddAsync(PickupAccess pickupAccess, CancellationToken cancellationToken) =>
        await db.PickupAccesses.AddAsync(pickupAccess, cancellationToken);
}
