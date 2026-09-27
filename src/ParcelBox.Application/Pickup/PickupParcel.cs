using ParcelBox.Application.Abstractions;
using ParcelBox.Application.Common;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Pickup;

public sealed class PickupParcelHandler(
    IParcelRepository parcels,
    ICompartmentRepository compartments,
    IPickupAccessRepository pickupAccesses,
    ILockerController lockerController,
    IPickupCodeService pickupCodes,
    IAppDbSession db)
{
    public async Task<OperationResult> HandleAsync(string trackingCode, string pickupCode, CancellationToken cancellationToken)
    {
        var parcel = await parcels.GetByTrackingCodeAsync(trackingCode, cancellationToken);
        if (parcel is null)
            return OperationResult.Failure("Parcel not found.");

        if (parcel.Status != ParcelStatus.Stored)
            return OperationResult.Failure("Parcel is not ready for pickup.");

        var access = await pickupAccesses.GetByParcelIdAsync(parcel.Id, cancellationToken);
        if (access is null)
            return OperationResult.Failure("Pickup access not found.");

        var now = DateTimeOffset.UtcNow;
        var validation = access.ValidateAttempt(pickupCodes.Hash(pickupCode), now);
        await db.SaveChangesAsync(cancellationToken);

        if (validation != PickupCodeValidation.Valid)
            return OperationResult.Failure($"Pickup code is {validation.ToString().ToLowerInvariant()}.");

        var compartment = await compartments.GetByParcelIdAsync(parcel.Id, cancellationToken);
        if (compartment is null)
            return OperationResult.Failure("Compartment not found.");

        var openResult = await lockerController.OpenAsync(compartment.LockerCode, compartment.Number, cancellationToken);
        if (openResult != LockerOpenResult.Opened)
            return OperationResult.Failure($"Locker could not be opened: {openResult}.");

        access.MarkUsed(now);
        parcel.MarkPickedUp(now);
        compartment.Release(parcel.Id);
        await db.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }
}
