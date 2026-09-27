using ParcelBox.Application.Abstractions.External;
using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Application.Abstractions.Security;
using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Lockers;
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

    public async Task<Result<StoreParcelResult, ParcelOperationError>> HandleAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        var parcel = await _parcels.GetByIdAsync(parcelId, cancellationToken);

        if (parcel is null)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(ParcelOperationError.NotFound);
        }

        if (!parcel.CanBeStored)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                ParcelOperationError.NotRegisteredForStorage);
        }

        var compartment = await _compartments.FindAvailableAsync(parcel.Size, cancellationToken);

        if (compartment is null)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                ParcelOperationError.NoCompatibleCompartment);
        }

        var openResult = await _lockerController.OpenAsync(
            compartment.LockerCode,
            compartment.Number,
            cancellationToken);

        if (openResult.IsFailure)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                MapLockerError(openResult.Error));
        }

        var now = _timeProvider.GetUtcNow();
        var storeResult = parcel.Store(compartment.LockerCode, compartment.Number, now);

        if (storeResult.IsFailure)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                MapParcelError(storeResult.Error));
        }

        var occupyResult = compartment.Occupy(parcel.Id);

        if (occupyResult.IsFailure)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                MapCompartmentError(occupyResult.Error));
        }

        var code = _pickupCodes.Generate();
        var accessResult = PickupAccess.Create(
            parcel.Id,
            _pickupCodes.Hash(code),
            now.AddHours(24),
            now);

        if (accessResult.IsFailure)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                ParcelOperationError.PickupAccessInvalid);
        }

        await _pickupAccesses.AddAsync(accessResult.Value, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var message = new OutboundMessage(
            parcel.RecipientPhone,
            $"Parcel {parcel.TrackingCode} is ready. Pickup code: {code}");

        var deliveryResult = await _messageGateway.SendAsync(message, cancellationToken);

        var deliveryStatus = deliveryResult.IsSuccess
            ? PickupMessageStatus.Delivered
            : PickupMessageStatus.Failed;

        var result = new StoreParcelResult(
            compartment.LockerCode,
            compartment.Number,
            deliveryStatus);

        return Result<StoreParcelResult, ParcelOperationError>.Success(result);
    }

    private static ParcelOperationError MapParcelError(ParcelError error)
    {
        return error switch
        {
            ParcelError.NotRegisteredForStorage => ParcelOperationError.NotRegisteredForStorage,
            ParcelError.StorageLocationRequired => ParcelOperationError.StorageLocationRequired,
            _ => ParcelOperationError.UnexpectedDomainFailure
        };
    }

    private static ParcelOperationError MapCompartmentError(CompartmentError error)
    {
        return error switch
        {
            CompartmentError.NotAvailable => ParcelOperationError.CompartmentUnavailable,
            _ => ParcelOperationError.UnexpectedDomainFailure
        };
    }

    private static ParcelOperationError MapLockerError(LockerControllerError error)
    {
        return error switch
        {
            LockerControllerError.Jammed => ParcelOperationError.LockerJammed,
            LockerControllerError.Unavailable => ParcelOperationError.LockerUnavailable,
            _ => ParcelOperationError.UnexpectedDomainFailure
        };
    }
}
