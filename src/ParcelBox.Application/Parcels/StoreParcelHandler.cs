using ParcelBox.Application.Abstractions;
using ParcelBox.Application.Common;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Parcels;

public sealed class StoreParcelHandler(
    IParcelRepository parcels,
    ICompartmentRepository compartments,
    IPickupAccessRepository pickupAccesses,
    ILockerController lockerController,
    IMessageGateway messageGateway,
    IPickupCodeService pickupCodes,
    IAppDbSession db)
{
    public async Task<OperationResult<StoreParcelResult>> HandleAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        var parcel = await parcels.GetByIdAsync(parcelId, cancellationToken);
        if (parcel is null)
            return OperationResult<StoreParcelResult>.Failure("Parcel not found.");

        if (parcel.Status != ParcelStatus.Registered)
            return OperationResult<StoreParcelResult>.Failure("Only a registered parcel can be stored.");

        var compartment = await compartments.FindAvailableAsync(parcel.Size, cancellationToken);
        if (compartment is null)
            return OperationResult<StoreParcelResult>.Failure("No compatible compartment is available.");

        var openResult = await lockerController.OpenAsync(
            compartment.LockerCode,
            compartment.Number,
            cancellationToken);

        if (openResult != LockerOpenResult.Opened)
            return OperationResult<StoreParcelResult>.Failure($"Locker could not be opened: {openResult}.");

        var now = DateTimeOffset.UtcNow;
        parcel.Store(compartment.LockerCode, compartment.Number, now);
        compartment.Occupy(parcel.Id);

        var code = pickupCodes.Generate();
        var access = PickupAccess.Create(parcel.Id, pickupCodes.Hash(code), now.AddHours(24));
        await pickupAccesses.AddAsync(access, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var delivered = await messageGateway.SendPickupCodeAsync(
            parcel.RecipientPhone,
            parcel.TrackingCode,
            code,
            cancellationToken);

        if (delivered)
            access.MarkMessageDelivered();
        else
            access.MarkMessageFailed();

        await db.SaveChangesAsync(cancellationToken);

        return OperationResult<StoreParcelResult>.Success(new StoreParcelResult(
            compartment.LockerCode,
            compartment.Number,
            access.MessageDelivery));
    }
}
