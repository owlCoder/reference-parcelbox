using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Parcels;
using ParcelBox.Application.Enums;

namespace ParcelBox.Application.Interfaces.Services;

public interface IParcelStorageService
{
    Task<Result<StoreParcelResult, ParcelOperationError>> StoreAsync(
        Guid parcelId,
        CancellationToken cancellationToken);
}
