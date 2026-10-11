using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Parcels.Enums;
using ParcelBox.Domain.Parcels.Models;

namespace ParcelBox.Infrastructure.Persistence.Repositories;

internal sealed class ParcelRepository : IParcelRepository
{
    private readonly ParcelBoxDbContext _db;

    public ParcelRepository(ParcelBoxDbContext db)
    {
        _db = db;
    }

    public Task<bool> TrackingCodeExistsAsync(string trackingCode, CancellationToken cancellationToken)
    {
        return _db.Parcels.AnyAsync(
            parcel => parcel.TrackingCode == trackingCode,
            cancellationToken);
    }

    public async Task<Parcel> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Parcels.SingleOrDefaultAsync(parcel => parcel.Id == id, cancellationToken) ?? new Parcel();
    }

    public async Task<Parcel> GetByTrackingCodeAsync(string trackingCode, CancellationToken cancellationToken)
    {
        return await _db.Parcels.SingleOrDefaultAsync(
            parcel => parcel.TrackingCode == trackingCode,
            cancellationToken) ?? new Parcel();
    }

    public Task<ClaimResult> TryBeginStorageAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(parcelId, ParcelStatus.Registered, ParcelStatus.StoragePending, cancellationToken);
    }

    public Task<ClaimResult> TryResetStorageAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(parcelId, ParcelStatus.StoragePending, ParcelStatus.Registered, cancellationToken);
    }

    public Task<ClaimResult> TryBeginPickupAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(parcelId, ParcelStatus.Stored, ParcelStatus.PickupPending, cancellationToken);
    }

    public Task<ClaimResult> TryResetPickupAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(parcelId, ParcelStatus.PickupPending, ParcelStatus.Stored, cancellationToken);
    }

    private async Task<ClaimResult> ChangeStatusAsync(Guid parcelId, ParcelStatus expected, ParcelStatus desired, CancellationToken cancellationToken)
    {
        try
        {
            var changed = await _db.Parcels
                .Where(parcel => parcel.Id == parcelId && parcel.Status == expected)
                .ExecuteUpdateAsync(setters => setters.SetProperty(parcel => parcel.Status, desired), cancellationToken);

            return changed == 1 ? ClaimResult.Claimed : ClaimResult.Conflict;
        }
        catch (DbUpdateException)
        {
            return ClaimResult.Unavailable;
        }
        catch (SqliteException)
        {
            return ClaimResult.Unavailable;
        }
    }

    public async Task AddAsync(Parcel parcel, CancellationToken cancellationToken)
    {
        await _db.Parcels.AddAsync(parcel, cancellationToken);
    }
}
