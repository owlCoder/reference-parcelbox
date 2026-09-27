using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Pickup;
using ParcelBox.Application.Enums;
using ParcelBox.Domain.Pickup.Models;

namespace ParcelBox.Application.Interfaces.Services;

public interface IPickupAccessService
{
    Task<Result<PickupAccessPreparation, PickupOperationError>> CreateAsync(
        Guid parcelId,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<Result<PickupAccess, PickupOperationError>> ValidateAsync(
        Guid parcelId,
        string pickupCode,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    void MarkUsed(PickupAccess access, DateTimeOffset now);
}
