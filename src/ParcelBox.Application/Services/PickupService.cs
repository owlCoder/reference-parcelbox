using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Pickup;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.External;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Security;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Services;

public sealed class PickupService : IPickupService
{
    private const int MaxFailedAttempts = 3;
    private static readonly TimeSpan AccessLifetime = TimeSpan.FromHours(24);

    private readonly IParcelRepository _parcels;
    private readonly IPickupAccessRepository _pickupAccesses;
    private readonly ILockerService _lockers;
    private readonly IMessageGateway _messageGateway;
    private readonly IPickupCodeService _pickupCodes;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public PickupService(
        IParcelRepository parcels,
        IPickupAccessRepository pickupAccesses,
        ILockerService lockers,
        IMessageGateway messageGateway,
        IPickupCodeService pickupCodes,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _parcels = parcels;
        _pickupAccesses = pickupAccesses;
        _lockers = lockers;
        _messageGateway = messageGateway;
        _pickupCodes = pickupCodes;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<PickupPreparation, PickupOperationError>> PrepareAccessAsync(
        Guid parcelId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (parcelId == Guid.Empty)
        {
            return Result<PickupPreparation, PickupOperationError>.Failure(
                PickupOperationError.ParcelNotFound);
        }

        var code = _pickupCodes.Generate();
        var access = new PickupAccess
        {
            Id = Guid.NewGuid(),
            ParcelId = parcelId,
            CodeHash = _pickupCodes.Hash(code),
            ExpiresAt = now.Add(AccessLifetime),
            FailedAttempts = 0,
            Status = PickupStatus.Active
        };

        await _pickupAccesses.AddAsync(access, cancellationToken);

        return Result<PickupPreparation, PickupOperationError>.Success(
            new PickupPreparation(code));
    }

    public async Task<PickupMessageStatus> SendReadyMessageAsync(
        Parcel parcel,
        string pickupCode,
        CancellationToken cancellationToken)
    {
        var message = new OutboundMessage(
            parcel.RecipientPhone,
            $"Parcel {parcel.TrackingCode} is ready. Pickup code: {pickupCode}");

        var result = await _messageGateway.SendAsync(message, cancellationToken);

        return result.IsSuccess
            ? PickupMessageStatus.Delivered
            : PickupMessageStatus.Failed;
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

        var access = await _pickupAccesses.GetByParcelIdAsync(parcel.Id, cancellationToken);

        if (access is null)
        {
            return Result<PickupOperationError>.Failure(
                PickupOperationError.PickupAccessNotFound);
        }

        if (access.Status == PickupStatus.Used)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.PickupCodeUsed);
        }

        if (access.Status == PickupStatus.Locked)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.PickupCodeLocked);
        }

        var now = _timeProvider.GetUtcNow();

        if (now >= access.ExpiresAt)
        {
            return Result<PickupOperationError>.Failure(PickupOperationError.PickupCodeExpired);
        }

        if (!_pickupCodes.Verify(pickupCode.Trim(), access.CodeHash))
        {
            access.FailedAttempts++;

            if (access.FailedAttempts >= MaxFailedAttempts)
            {
                access.Status = PickupStatus.Locked;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<PickupOperationError>.Failure(
                access.Status == PickupStatus.Locked
                    ? PickupOperationError.PickupCodeLocked
                    : PickupOperationError.PickupCodeInvalid);
        }

        var openedCompartment = await _lockers.OpenForPickupAsync(
            parcel.Id,
            cancellationToken);

        if (openedCompartment.IsFailure)
        {
            var error = openedCompartment.Error switch
            {
                LockerOperationError.CompartmentNotFound => PickupOperationError.CompartmentNotFound,
                LockerOperationError.Jammed => PickupOperationError.LockerJammed,
                LockerOperationError.Unavailable => PickupOperationError.LockerUnavailable,
                _ => PickupOperationError.CompartmentStateConflict
            };

            return Result<PickupOperationError>.Failure(error);
        }

        var releaseResult = _lockers.Release(openedCompartment.Value, parcel.Id);

        if (releaseResult.IsFailure)
        {
            return Result<PickupOperationError>.Failure(
                PickupOperationError.CompartmentStateConflict);
        }

        access.Status = PickupStatus.Used;
        access.UsedAt = now;
        parcel.Status = ParcelStatus.PickedUp;
        parcel.PickedUpAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PickupOperationError>.Success();
    }
}
