using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class ParcelRepositoryFake : IParcelRepository
{
    public List<Parcel> Items { get; } = [];

    public Task<bool> TrackingCodeExistsAsync(
        string trackingCode,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Items.Any(x => x.TrackingCode == trackingCode));
    }

    public Task<Parcel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Items.SingleOrDefault(x => x.Id == id));
    }

    public Task<Parcel?> GetByTrackingCodeAsync(
        string trackingCode,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Items.SingleOrDefault(x => x.TrackingCode == trackingCode));
    }

    public Task AddAsync(Parcel parcel, CancellationToken cancellationToken)
    {
        Items.Add(parcel);
        return Task.CompletedTask;
    }
}
