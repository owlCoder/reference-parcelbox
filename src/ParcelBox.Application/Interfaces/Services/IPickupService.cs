using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Pickup;
using ParcelBox.Application.Enums;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Interfaces.Services;

public interface IPickupService
{
    Task<Result<PickupPreparation, PickupOperationError>> PrepareAccessAsync(
        Guid parcelId,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<PickupMessageStatus> SendReadyMessageAsync(
        Parcel parcel,
        string pickupCode,
        CancellationToken cancellationToken);

    Task<Result<PickupOperationError>> CompletePickupAsync(
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken);
}
