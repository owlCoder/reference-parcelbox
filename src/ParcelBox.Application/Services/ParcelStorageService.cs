using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Notifications;
using ParcelBox.Application.DTOs.Parcels;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Persistence;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Services;
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

    public ParcelStorageService(
        IParcelRepository parcels,
        ILockerService lockers,
        IPickupAccessService pickupAccess,
        INotificationService notifications,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _parcels = parcels;
        _lockers = lockers;
        _pickupAccess = pickupAccess;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<StoreParcelResult, ParcelOperationError>> StoreAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        var parcel = await _parcels.GetByIdAsync(parcelId, cancellationToken);

        if (parcel is null)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                ParcelOperationError.NotFound);
        }

        if (parcel.Status != ParcelStatus.Registered)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                ParcelOperationError.NotRegisteredForStorage);
        }

        var assignment = await _lockers.AssignAsync(parcel.Id, parcel.Size, cancellationToken);

        if (assignment.IsFailure)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                MapLockerError(assignment.Error));
        }

        var now = _timeProvider.GetUtcNow();
        var preparation = await _pickupAccess.CreateAsync(parcel.Id, now, cancellationToken);

        if (preparation.IsFailure)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                ParcelOperationError.PickupAccessInvalid);
        }

        parcel.LockerCode = assignment.Value.LockerCode;
        parcel.CompartmentNumber = assignment.Value.Number;
        parcel.StoredAt = now;
        parcel.Status = ParcelStatus.Stored;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var notification = new PickupNotification(
            parcel.RecipientPhone,
            parcel.TrackingCode,
            preparation.Value.Code);

        var messageStatus = await _notifications.SendPickupCodeAsync(
            notification,
            cancellationToken);

        return Result<StoreParcelResult, ParcelOperationError>.Success(
            new StoreParcelResult(
                assignment.Value.LockerCode,
                assignment.Value.Number,
                messageStatus));
    }

    private static ParcelOperationError MapLockerError(LockerOperationError error)
    {
        return error switch
        {
            LockerOperationError.NoCompatibleCompartment => ParcelOperationError.NoCompatibleCompartment,
            LockerOperationError.Jammed => ParcelOperationError.LockerJammed,
            LockerOperationError.Unavailable => ParcelOperationError.LockerUnavailable,
            _ => ParcelOperationError.CompartmentUnavailable
        };
    }
}
