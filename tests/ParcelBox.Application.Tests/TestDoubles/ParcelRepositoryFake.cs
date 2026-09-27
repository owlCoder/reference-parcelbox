using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class ParcelRepositoryFake : IParcelRepository
{
    private readonly Parcel _parcel;

    public ParcelRepositoryFake(Parcel parcel)
    {
        _parcel = parcel;
    }

    public Task<bool> TrackingCodeExistsAsync(
        string trackingCode,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }

    public Task<Parcel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult<Parcel?>(_parcel.Id == id ? _parcel : null);
    }

    public Task<Parcel?> GetByTrackingCodeAsync(
        string trackingCode,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<Parcel?>(
            _parcel.TrackingCode == trackingCode ? _parcel : null);
    }

    public Task AddAsync(Parcel parcel, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
