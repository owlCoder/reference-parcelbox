using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Parcels;
using ParcelBox.Application.Enums;

namespace ParcelBox.Application.Interfaces.Services;

public interface IParcelService
{
    Task<Result<ParcelDetailsDto, ParcelOperationError>> RegisterAsync(RegisterParcelInputDto input, CancellationToken cancellationToken);

    Task<Result<ParcelDetailsDto, ParcelOperationError>> GetAsync(Guid id, CancellationToken cancellationToken);
}
