using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Notifications;
using ParcelBox.Application.DTOs.Parcels;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Persistence;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Parcels.Enums;

namespace ParcelBox.Application.Services;

public sealed class ParcelStorageService : IParcelStorageService
{
    private readonly IParcelRepository _parcels;
    private readonly ILockerService _lockers;
    private readonly IPickupAccessService _pickupAccess;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ParcelStorageService(IParcelRepository parcels, ILockerService lockers, IPickupAccessService pickupAccess, INotificationService notifications, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _parcels = parcels;
        _lockers = lockers;
        _pickupAccess = pickupAccess;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<StoreParcelResultDto, ParcelOperationError>> StoreAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        var parcel = await _parcels.GetByIdAsync(parcelId, cancellationToken);

        if (parcel.Id == Guid.Empty)
        {
            return Result<StoreParcelResultDto, ParcelOperationError>.Failure(
                ParcelOperationError.NotFound);
        }

        if (parcel.Status != ParcelStatus.Registered)
        {
            return Result<StoreParcelResultDto, ParcelOperationError>.Failure(
                ParcelOperationError.NotRegisteredForStorage);
        }

        var claim = await _parcels.TryBeginStorageAsync(parcel.Id, cancellationToken);

        if (claim == ClaimResult.Unavailable)
        {
            return Result<StoreParcelResultDto, ParcelOperationError>.Failure(ParcelOperationError.PersistenceUnavailable);
        }

        if (claim != ClaimResult.Claimed)
        {
            return Result<StoreParcelResultDto, ParcelOperationError>.Failure(ParcelOperationError.NotRegisteredForStorage);
        }

        parcel.Status = ParcelStatus.StoragePending;
        var assignment = await _lockers.AssignAsync(parcel.Id, parcel.Size, cancellationToken);

        if (assignment.IsFailure)
        {
            if (assignment.Error == LockerOperationError.NoCompatibleCompartment)
            {
                var reset = await _parcels.TryResetStorageAsync(parcel.Id, cancellationToken);

                if (reset == ClaimResult.Claimed)
                {
                    parcel.Status = ParcelStatus.Registered;
                    return Result<StoreParcelResultDto, ParcelOperationError>.Failure(ParcelOperationError.NoCompatibleCompartment);
                }
            }

            // A physical opening may have occurred. Leave the durable claim in place for reconciliation.
            return Result<StoreParcelResultDto, ParcelOperationError>.Failure(ParcelOperationError.StorageRecoveryRequired);
        }

        var now = _timeProvider.GetUtcNow();
        var preparation = await _pickupAccess.CreateAsync(parcel.Id, now, cancellationToken);

        if (preparation.IsFailure)
        {
            return Result<StoreParcelResultDto, ParcelOperationError>.Failure(
                ParcelOperationError.StorageRecoveryRequired);
        }

        parcel.LockerCode = assignment.Value.LockerCode;
        parcel.CompartmentNumber = assignment.Value.Number;
        parcel.StoredAt = now;
        parcel.Status = ParcelStatus.Stored;

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
        {
            // The claim remains durable; do not reassign a compartment that may already be open.
            parcel.Status = ParcelStatus.StoragePending;
            assignment.Value.Status = CompartmentStatus.Reserved;
            return Result<StoreParcelResultDto, ParcelOperationError>.Failure(ParcelOperationError.StorageRecoveryRequired);
        }

        var notification = new PickupNotificationDto(
            parcel.RecipientPhone,
            parcel.TrackingCode,
            preparation.Value.Code);

        var messageStatus = await _notifications.SendPickupCodeAsync(
            notification,
            cancellationToken);

        return Result<StoreParcelResultDto, ParcelOperationError>.Success(
            new StoreParcelResultDto(
                assignment.Value.LockerCode,
                assignment.Value.Number,
                messageStatus));
    }
}
