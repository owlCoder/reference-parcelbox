using ParcelBox.Application.Common.Results;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Persistence;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Parcels.Enums;
using ParcelBox.Domain.Pickup.Enums;

namespace ParcelBox.Application.Services;

public sealed class PickupService : IPickupService
{
    private readonly IParcelRepository _parcels;
    private readonly ILockerService _lockers;
    private readonly IPickupAccessService _pickupAccess;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public PickupService(IParcelRepository parcels, ILockerService lockers, IPickupAccessService pickupAccess, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _parcels = parcels;
        _lockers = lockers;
        _pickupAccess = pickupAccess;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<PickupOperationError>> CompletePickupAsync(string trackingCode, string pickupCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
        {
            return Result<PickupOperationError>.Failure(
                PickupOperationError.TrackingCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(pickupCode))
        {
            return Result<PickupOperationError>.Failure(
                PickupOperationError.PickupCodeRequired);
        }

        var parcel = await _parcels.GetByTrackingCodeAsync(
            trackingCode.Trim(),
            cancellationToken);

        if (parcel.Id == Guid.Empty)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.ParcelNotFound);
        }

        if (parcel.Status != ParcelStatus.Stored)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.ParcelNotStored);
        }

        var now = _timeProvider.GetUtcNow();
        var access = await _pickupAccess.ValidateAsync(
            parcel.Id,
            pickupCode,
            now,
            cancellationToken);

        if (access.IsFailure)
        {
            if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            {
                return Result<PickupOperationError>.Failure(PickupOperationError.PersistenceUnavailable);
            }

            return Result<PickupOperationError>.Failure(access.Error);
        }

        var claim = await _parcels.TryBeginPickupAsync(parcel.Id, cancellationToken);

        if (claim == ClaimResult.Unavailable)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.PersistenceUnavailable);
        }

        if (claim != ClaimResult.Claimed)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.ParcelNotStored);
        }

        parcel.Status = ParcelStatus.PickupPending;
        var openedCompartment = await _lockers.OpenForPickupAsync(parcel.Id, cancellationToken);

        if (openedCompartment.IsFailure)
        {
            if (openedCompartment.Error == LockerOperationError.CompartmentNotFound ||
                openedCompartment.Error == LockerOperationError.CompartmentStateConflict)
            {
                var reset = await _parcels.TryResetPickupAsync(parcel.Id, cancellationToken);

                if (reset == ClaimResult.Claimed)
                {
                    parcel.Status = ParcelStatus.Stored;
                    return Result<PickupOperationError>.Failure(MapLockerError(openedCompartment.Error));
                }
            }

            // If the controller was contacted, physical state is uncertain; require reconciliation.
            return Result<PickupOperationError>.Failure(PickupOperationError.PickupRecoveryRequired);
        }

        var releaseResult = _lockers.Release(openedCompartment.Value, parcel.Id);

        if (releaseResult.IsFailure)
        {
            return Result<PickupOperationError>.Failure(
                PickupOperationError.PickupRecoveryRequired);
        }

        _pickupAccess.MarkUsed(access.Value, now);
        parcel.Status = ParcelStatus.PickedUp;
        parcel.PickedUpAt = now;

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
        {
            parcel.Status = ParcelStatus.PickupPending;
            openedCompartment.Value.Status = CompartmentStatus.Occupied;
            openedCompartment.Value.ParcelId = parcel.Id;
            access.Value.Status = PickupStatus.Active;
            access.Value.UsedAt = null;
            return Result<PickupOperationError>.Failure(PickupOperationError.PickupRecoveryRequired);
        }

        return Result<PickupOperationError>.Success();
    }

    private static PickupOperationError MapLockerError(LockerOperationError error)
    {
        return error switch
        {
            LockerOperationError.CompartmentNotFound => PickupOperationError.CompartmentNotFound,
            LockerOperationError.Jammed => PickupOperationError.LockerJammed,
            LockerOperationError.Unavailable => PickupOperationError.LockerUnavailable,
            _ => PickupOperationError.CompartmentStateConflict
        };
    }
}
