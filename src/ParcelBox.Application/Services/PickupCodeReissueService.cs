using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Notifications;
using ParcelBox.Application.DTOs.Pickup;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Persistence;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Parcels.Enums;

namespace ParcelBox.Application.Services;

public sealed class PickupCodeReissueService : IPickupCodeReissueService
{
    private readonly IParcelRepository _parcels;
    private readonly IPickupAccessService _pickupAccess;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public PickupCodeReissueService(IParcelRepository parcels, IPickupAccessService pickupAccess, INotificationService notifications, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _parcels = parcels;
        _pickupAccess = pickupAccess;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<PickupCodeReissueResultDto, PickupOperationError>> ReissueAsync(string trackingCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
        {
            return Result<PickupCodeReissueResultDto, PickupOperationError>.Failure(
                PickupOperationError.TrackingCodeRequired);
        }

        var parcel = await _parcels.GetByTrackingCodeAsync(
            trackingCode.Trim(),
            cancellationToken);

        if (parcel.Id == Guid.Empty)
        {
            return Result<PickupCodeReissueResultDto, PickupOperationError>.Failure(
                PickupOperationError.ParcelNotFound);
        }

        if (parcel.Status != ParcelStatus.Stored)
        {
            return Result<PickupCodeReissueResultDto, PickupOperationError>.Failure(
                PickupOperationError.ParcelNotStored);
        }

        var now = _timeProvider.GetUtcNow();
        var reissue = await _pickupAccess.ReissueAsync(
            parcel.Id,
            now,
            cancellationToken);

        if (reissue.IsFailure)
        {
            return Result<PickupCodeReissueResultDto, PickupOperationError>.Failure(
                reissue.Error);
        }

        // Save before sending so the recipient never receives a code that was not stored.
        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
        {
            return Result<PickupCodeReissueResultDto, PickupOperationError>.Failure(
                PickupOperationError.PersistenceUnavailable);
        }

        var notification = new PickupNotificationDto(
            parcel.RecipientPhone,
            parcel.TrackingCode,
            reissue.Value.Code);

        var messageStatus = await _notifications.SendPickupCodeAsync(
            notification,
            cancellationToken);

        return Result<PickupCodeReissueResultDto, PickupOperationError>.Success(
            new PickupCodeReissueResultDto(
                parcel.TrackingCode,
                messageStatus));
    }
}
