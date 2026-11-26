using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Pickup;
using ParcelBox.Application.Enums;

namespace ParcelBox.Application.Interfaces.Services;

public interface IPickupCodeReissueService
{
    Task<Result<PickupCodeReissueResultDto, PickupOperationError>> ReissueAsync(string trackingCode, CancellationToken cancellationToken);
}
