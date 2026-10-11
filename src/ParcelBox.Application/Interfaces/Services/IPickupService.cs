using ParcelBox.Application.Common.Results;
using ParcelBox.Application.Enums;

namespace ParcelBox.Application.Interfaces.Services;

public interface IPickupService
{
    Task<Result<PickupOperationError>> CompletePickupAsync(string trackingCode, string pickupCode, CancellationToken cancellationToken);
}
