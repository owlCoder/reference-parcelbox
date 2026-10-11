using ParcelBox.Web.Common.Results;
using ParcelBox.Web.Contracts;

namespace ParcelBox.Web.Interfaces;

public interface IParcelBoxApiClient : IServiceHealthClient
{
    Task<ClientResult<ParcelDetailsDto>> RegisterAsync(RegisterParcelRequestDto request, CancellationToken cancellationToken = default);

    Task<ClientResult<ParcelDetailsDto>> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientResult<StoreParcelResultDto>> StoreAsync(Guid parcelId, CancellationToken cancellationToken = default);

    Task<ClientResult<List<CompartmentDetailsDto>>> ListCompartmentsAsync(CancellationToken cancellationToken = default);

    Task<ClientResult> PickupAsync(string trackingCode, string pickupCode, CancellationToken cancellationToken = default);
}
