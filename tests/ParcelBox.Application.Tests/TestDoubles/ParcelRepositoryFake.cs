using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Parcels.Enums;
using ParcelBox.Domain.Parcels.Models;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class ParcelRepositoryFake : IParcelRepository
{
    public List<Parcel> Items { get; } = [];

    public Task<bool> TrackingCodeExistsAsync(string trackingCode, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Items.Any(parcel => parcel.TrackingCode == trackingCode));
    }

    public Task<Parcel> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Items.SingleOrDefault(parcel => parcel.Id == id) ?? new Parcel());
    }

    public Task<Parcel> GetByTrackingCodeAsync(string trackingCode, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Items.SingleOrDefault(parcel => parcel.TrackingCode == trackingCode) ?? new Parcel());
    }

    public Task<ClaimResult> TryBeginStorageAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(parcelId, ParcelStatus.Registered, ParcelStatus.StoragePending);
    }

    public Task<ClaimResult> TryResetStorageAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(parcelId, ParcelStatus.StoragePending, ParcelStatus.Registered);
    }

    public Task<ClaimResult> TryBeginPickupAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(parcelId, ParcelStatus.Stored, ParcelStatus.PickupPending);
    }

    public Task<ClaimResult> TryResetPickupAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(parcelId, ParcelStatus.PickupPending, ParcelStatus.Stored);
    }

    private Task<ClaimResult> ChangeStatusAsync(Guid parcelId, ParcelStatus expected, ParcelStatus desired)
    {
        lock (Items)
        {
            var parcel = Items.FirstOrDefault(item => item.Id == parcelId);

            if (parcel is null || parcel.Status != expected)
            {
                return Task.FromResult(ClaimResult.Conflict);
            }

            parcel.Status = desired;
            return Task.FromResult(ClaimResult.Claimed);
        }
    }

    public Task AddAsync(Parcel parcel, CancellationToken cancellationToken)
    {
        Items.Add(parcel);
        return Task.CompletedTask;
    }
}
