using ParcelBox.Application.Abstractions.External;
using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Application.Abstractions.Security;
using ParcelBox.Application.Lockers;
using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Parcels;

public sealed class StoreParcelHandler
{
    private readonly IParcelRepository _parcels;
    private readonly ICompartmentRepository _compartments;
    private readonly IPickupAccessRepository _pickupAccesses;
    private readonly ILockerController _lockerController;
    private readonly IMessageGateway _messageGateway;
    private readonly IPickupCodeService _pickupCodes;
    private readonly IAppDbSession _db;
    private readonly TimeProvider _timeProvider;

    public StoreParcelHandler(
        IParcelRepository parcels,
        ICompartmentRepository compartments,
        IPickupAccessRepository pickupAccesses,
        ILockerController lockerController,
        IMessageGateway messageGateway,
        IPickupCodeService pickupCodes,
        IAppDbSession db,
        TimeProvider timeProvider)
    {
        _parcels = parcels;
        _compartments = compartments;
        _pickupAccesses = pickupAccesses;
        _lockerController = lockerController;
        _messageGateway = messageGateway;
        _pickupCodes = pickupCodes;
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<Result<StoreParcelResult>> HandleAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        var parcel = await _parcels.GetByIdAsync(parcelId, cancellationToken);

        if (parcel is null)
        {
            return Result<StoreParcelResult>.Failure(ParcelApplicationErrors.NotFound);
        }

        if (!parcel.CanBeStored)
        {
            return Result<StoreParcelResult>.Failure(ParcelErrors.NotRegisteredForStorage);
        }

        var sizeResult = CompartmentSizeMapping.ToCompartmentSize(parcel.Size);

        if (sizeResult.IsFailure)
        {
            return Result<StoreParcelResult>.Failure(sizeResult.Error);
        }

        var compartment = await _compartments.FindAvailableAsync(
            sizeResult.Value,
            cancellationToken);

        if (compartment is null)
        {
            return Result<StoreParcelResult>.Failure(ParcelApplicationErrors.NoCompatibleCompartment);
        }

        var openResult = await _lockerController.OpenAsync(
            compartment.LockerCode,
            compartment.Number,
            cancellationToken);

        if (openResult.IsFailure)
        {
            return Result<StoreParcelResult>.Failure(openResult.Error);
        }

        var now = _timeProvider.GetUtcNow();
        var storeResult = parcel.Store(compartment.LockerCode, compartment.Number, now);

        if (storeResult.IsFailure)
        {
            return Result<StoreParcelResult>.Failure(storeResult.Error);
        }

        var occupyResult = compartment.Occupy(parcel.Id);

        if (occupyResult.IsFailure)
        {
            return Result<StoreParcelResult>.Failure(occupyResult.Error);
        }

        var code = _pickupCodes.Generate();
        var accessResult = PickupAccess.Create(
            parcel.Id,
            _pickupCodes.Hash(code),
            now.AddHours(24));

        if (accessResult.IsFailure)
        {
            return Result<StoreParcelResult>.Failure(accessResult.Error);
        }

        var access = accessResult.Value;
        await _pickupAccesses.AddAsync(access, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var deliveryResult = await _messageGateway.SendPickupCodeAsync(
            parcel.RecipientPhone,
            parcel.TrackingCode,
            code,
            cancellationToken);

        if (deliveryResult.IsSuccess)
        {
            access.MarkMessageDelivered();
        }
        else
        {
            access.MarkMessageFailed();
        }

        await _db.SaveChangesAsync(cancellationToken);

        var result = new StoreParcelResult(
            compartment.LockerCode,
            compartment.Number,
            access.MessageDelivery);

        return Result<StoreParcelResult>.Success(result);
    }
}
