using ParcelBox.Application.Abstractions.External;
using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Application.Abstractions.Security;
using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Pickup;

public sealed class PickupParcelHandler
{
    private readonly IParcelRepository _parcels;
    private readonly ICompartmentRepository _compartments;
    private readonly IPickupAccessRepository _pickupAccesses;
    private readonly ILockerController _lockerController;
    private readonly IPickupCodeService _pickupCodes;
    private readonly IAppDbSession _db;
    private readonly TimeProvider _timeProvider;

    public PickupParcelHandler(
        IParcelRepository parcels,
        ICompartmentRepository compartments,
        IPickupAccessRepository pickupAccesses,
        ILockerController lockerController,
        IPickupCodeService pickupCodes,
        IAppDbSession db,
        TimeProvider timeProvider)
    {
        _parcels = parcels;
        _compartments = compartments;
        _pickupAccesses = pickupAccesses;
        _lockerController = lockerController;
        _pickupCodes = pickupCodes;
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<Result<PickupOperationError>> HandleAsync(
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.TrackingCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(pickupCode))
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.PickupCodeRequired);
        }

        var parcel = await _parcels.GetByTrackingCodeAsync(trackingCode, cancellationToken);

        if (parcel is null)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.ParcelNotFound);
        }

        if (!parcel.CanBePickedUp)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.ParcelNotStored);
        }

        var access = await _pickupAccesses.GetByParcelIdAsync(parcel.Id, cancellationToken);

        if (access is null)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.PickupAccessNotFound);
        }

        var now = _timeProvider.GetUtcNow();
        var codeMatches = _pickupCodes.Verify(pickupCode, access.CodeHash);
        var validation = access.ValidateAttempt(codeMatches, now);
        await _db.SaveChangesAsync(cancellationToken);

        if (validation != PickupCodeValidation.Valid)
        {
            return Result<PickupOperationError>.Failure(MapValidation(validation));
        }

        var compartment = await _compartments.GetByParcelIdAsync(parcel.Id, cancellationToken);

        if (compartment is null)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.CompartmentNotFound);
        }

        var openResult = await _lockerController.OpenAsync(
            compartment.LockerCode,
            compartment.Number,
            cancellationToken);

        if (openResult.IsFailure)
        {
            return Result<PickupOperationError>.Failure(MapLockerError(openResult.Error));
        }

        var markUsedResult = access.MarkUsed(now);

        if (markUsedResult.IsFailure)
        {
            return Result<PickupOperationError>.Failure(MapPickupAccessError(markUsedResult.Error));
        }

        var pickupResult = parcel.MarkPickedUp(now);

        if (pickupResult.IsFailure)
        {
            return Result<PickupOperationError>.Failure(MapParcelError(pickupResult.Error));
        }

        var releaseResult = compartment.Release(parcel.Id);

        if (releaseResult.IsFailure)
        {
            return Result<PickupOperationError>.Failure(MapCompartmentError(releaseResult.Error));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result<PickupOperationError>.Success();
    }

    private static PickupOperationError MapValidation(PickupCodeValidation validation)
    {
        return validation switch
        {
            PickupCodeValidation.Invalid => PickupOperationError.PickupCodeInvalid,
            PickupCodeValidation.Expired => PickupOperationError.PickupCodeExpired,
            PickupCodeValidation.Locked => PickupOperationError.PickupCodeLocked,
            PickupCodeValidation.Used => PickupOperationError.PickupCodeUsed,
            _ => PickupOperationError.UnexpectedDomainFailure
        };
    }

    private static PickupOperationError MapLockerError(LockerControllerError error)
    {
        return error switch
        {
            LockerControllerError.Jammed => PickupOperationError.LockerJammed,
            LockerControllerError.Unavailable => PickupOperationError.LockerUnavailable,
            _ => PickupOperationError.UnexpectedDomainFailure
        };
    }

    private static PickupOperationError MapPickupAccessError(PickupAccessError error)
    {
        return error switch
        {
            PickupAccessError.NotActive => PickupOperationError.PickupAccessNotActive,
            PickupAccessError.Expired => PickupOperationError.PickupAccessExpired,
            _ => PickupOperationError.UnexpectedDomainFailure
        };
    }

    private static PickupOperationError MapParcelError(ParcelError error)
    {
        return error switch
        {
            ParcelError.NotStoredForPickup => PickupOperationError.ParcelNotStored,
            _ => PickupOperationError.UnexpectedDomainFailure
        };
    }

    private static PickupOperationError MapCompartmentError(CompartmentError error)
    {
        return error switch
        {
            CompartmentError.NotOccupiedByParcel => PickupOperationError.CompartmentStateConflict,
            _ => PickupOperationError.UnexpectedDomainFailure
        };
    }
}
