using ParcelBox.Application.Abstractions.External;
using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Application.Abstractions.Security;
using ParcelBox.Domain.Common.Results;
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

    public async Task<Result> HandleAsync(
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
        {
            return Result.Failure(PickupApplicationErrors.TrackingCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(pickupCode))
        {
            return Result.Failure(PickupApplicationErrors.PickupCodeRequired);
        }

        var parcel = await _parcels.GetByTrackingCodeAsync(trackingCode, cancellationToken);

        if (parcel is null)
        {
            return Result.Failure(ParcelApplicationErrors.NotFound);
        }

        if (!parcel.CanBePickedUp)
        {
            return Result.Failure(ParcelErrors.NotStoredForPickup);
        }

        var access = await _pickupAccesses.GetByParcelIdAsync(parcel.Id, cancellationToken);

        if (access is null)
        {
            return Result.Failure(PickupApplicationErrors.AccessNotFound);
        }

        var now = _timeProvider.GetUtcNow();
        var validation = access.ValidateAttempt(_pickupCodes.Hash(pickupCode), now);
        await _db.SaveChangesAsync(cancellationToken);

        if (validation != PickupCodeValidation.Valid)
        {
            return Result.Failure(PickupApplicationErrors.FromValidation(validation));
        }

        var compartment = await _compartments.GetByParcelIdAsync(parcel.Id, cancellationToken);

        if (compartment is null)
        {
            return Result.Failure(PickupApplicationErrors.CompartmentNotFound);
        }

        var openResult = await _lockerController.OpenAsync(
            compartment.LockerCode,
            compartment.Number,
            cancellationToken);

        if (openResult.IsFailure)
        {
            return openResult;
        }

        var markUsedResult = access.MarkUsed(now);

        if (markUsedResult.IsFailure)
        {
            return markUsedResult;
        }

        var pickupResult = parcel.MarkPickedUp(now);

        if (pickupResult.IsFailure)
        {
            return pickupResult;
        }

        var releaseResult = compartment.Release(parcel.Id);

        if (releaseResult.IsFailure)
        {
            return releaseResult;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
