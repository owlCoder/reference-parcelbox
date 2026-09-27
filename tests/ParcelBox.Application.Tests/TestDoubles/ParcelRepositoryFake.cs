using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Parcels.Models;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class ParcelRepositoryFake : IParcelRepository
{
    public List<Parcel> Items { get; } = [];

    public Task<bool> TrackingCodeExistsAsync(
        string trackingCode,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Items.Any(parcel => parcel.TrackingCode == trackingCode));
    }

    public Task<Parcel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Items.SingleOrDefault(parcel => parcel.Id == id));
    }

    public Task<Parcel?> GetByTrackingCodeAsync(
        string trackingCode,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Items.SingleOrDefault(parcel => parcel.TrackingCode == trackingCode));
    }

    public Task AddAsync(Parcel parcel, CancellationToken cancellationToken)
    {
        Items.Add(parcel);
        return Task.CompletedTask;
    }
}
