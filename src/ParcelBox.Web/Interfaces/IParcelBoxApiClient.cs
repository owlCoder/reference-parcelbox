using ParcelBox.Web.Common.Results;
using ParcelBox.Web.Contracts;

namespace ParcelBox.Web.Interfaces;

public interface IParcelBoxApiClient : IServiceHealthClient
{
    Task<ClientResult<ParcelDetails>> RegisterAsync(RegisterParcelRequest request, CancellationToken cancellationToken = default);

    Task<ClientResult<ParcelDetails>> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientResult<StoreParcelResult>> StoreAsync(Guid parcelId, CancellationToken cancellationToken = default);

    Task<ClientResult<List<CompartmentDetails>>> ListCompartmentsAsync(CancellationToken cancellationToken = default);

    Task<ClientResult> PickupAsync(string trackingCode, string pickupCode, CancellationToken cancellationToken = default);
}
