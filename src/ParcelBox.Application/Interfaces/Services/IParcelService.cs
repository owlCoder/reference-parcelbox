using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Parcels;
using ParcelBox.Application.Enums;

namespace ParcelBox.Application.Interfaces.Services;

public interface IParcelService
{
    Task<Result<ParcelDetails, ParcelOperationError>> RegisterAsync(
        RegisterParcelInput input,
        CancellationToken cancellationToken);

    Task<Result<ParcelDetails, ParcelOperationError>> GetAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Result<StoreParcelResult, ParcelOperationError>> StoreAsync(
        Guid id,
        CancellationToken cancellationToken);
}
