using ParcelBox.Application.Common.Results;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Persistence;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Parcels.Enums;

namespace ParcelBox.Application.Services;

public sealed class PickupService : IPickupService
{
    private readonly IParcelRepository _parcels;
    private readonly ILockerService _lockers;
    private readonly IPickupAccessService _pickupAccess;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public PickupService(
        IParcelRepository parcels,
        ILockerService lockers,
        IPickupAccessService pickupAccess,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _parcels = parcels;
        _lockers = lockers;
        _pickupAccess = pickupAccess;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<PickupOperationError>> CompletePickupAsync(
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken)
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

        if (parcel is null)
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<PickupOperationError>.Failure(access.Error);
        }

        var openedCompartment = await _lockers.OpenForPickupAsync(
            parcel.Id,
            cancellationToken);

        if (openedCompartment.IsFailure)
        {
            return Result<PickupOperationError>.Failure(
                MapLockerError(openedCompartment.Error));
        }

        var releaseResult = _lockers.Release(openedCompartment.Value, parcel.Id);

        if (releaseResult.IsFailure)
        {
            return Result<PickupOperationError>.Failure(
                PickupOperationError.CompartmentStateConflict);
        }

        _pickupAccess.MarkUsed(access.Value, now);
        parcel.Status = ParcelStatus.PickedUp;
        parcel.PickedUpAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
