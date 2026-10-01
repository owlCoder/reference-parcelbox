using ParcelBox.Web.Contracts;

namespace ParcelBox.Web.Interfaces;

public interface IParcelBoxApiClient : IServiceHealthClient
{
    Task<(ParcelDetails? Value, string? Error)> RegisterAsync(
        RegisterParcelRequest request,
        CancellationToken cancellationToken = default);

    Task<(ParcelDetails? Value, string? Error)> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<(StoreParcelResult? Value, string? Error)> StoreAsync(
        Guid parcelId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<CompartmentDetails>? Value, string? Error)> ListCompartmentsAsync(
        CancellationToken cancellationToken = default);

    Task<string?> PickupAsync(
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken = default);
}
