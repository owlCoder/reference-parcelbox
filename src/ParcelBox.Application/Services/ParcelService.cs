using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Parcels;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Services;

public sealed class ParcelService : IParcelService
{
    private readonly IParcelRepository _parcels;
    private readonly ILockerService _lockers;
    private readonly IPickupService _pickup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ParcelService(
        IParcelRepository parcels,
        ILockerService lockers,
        IPickupService pickup,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _parcels = parcels;
        _lockers = lockers;
        _pickup = pickup;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<ParcelDetails, ParcelOperationError>> RegisterAsync(
        RegisterParcelInput input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.TrackingCode))
        {
            return Result<ParcelDetails, ParcelOperationError>.Failure(
                ParcelOperationError.TrackingCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(input.RecipientPhone))
        {
            return Result<ParcelDetails, ParcelOperationError>.Failure(
                ParcelOperationError.RecipientPhoneRequired);
        }

        if (!Enum.IsDefined(input.Size))
        {
            return Result<ParcelDetails, ParcelOperationError>.Failure(
                ParcelOperationError.InvalidSize);
        }

        var trackingCode = input.TrackingCode.Trim();

        if (await _parcels.TrackingCodeExistsAsync(trackingCode, cancellationToken))
        {
            return Result<ParcelDetails, ParcelOperationError>.Failure(
                ParcelOperationError.TrackingCodeAlreadyExists);
        }

        var parcel = new Parcel
        {
            Id = Guid.NewGuid(),
            TrackingCode = trackingCode,
            RecipientPhone = input.RecipientPhone.Trim(),
            Size = input.Size,
            Status = ParcelStatus.Registered,
            CreatedAt = _timeProvider.GetUtcNow()
        };

        await _parcels.AddAsync(parcel, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ParcelDetails, ParcelOperationError>.Success(ToDetails(parcel));
    }

    public async Task<Result<ParcelDetails, ParcelOperationError>> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var parcel = await _parcels.GetByIdAsync(id, cancellationToken);

        return parcel is null
            ? Result<ParcelDetails, ParcelOperationError>.Failure(ParcelOperationError.NotFound)
            : Result<ParcelDetails, ParcelOperationError>.Success(ToDetails(parcel));
    }

    public async Task<Result<StoreParcelResult, ParcelOperationError>> StoreAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var parcel = await _parcels.GetByIdAsync(id, cancellationToken);

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

        var now = _timeProvider.GetUtcNow();
        var preparation = await _pickup.PrepareAccessAsync(parcel.Id, now, cancellationToken);

        if (preparation.IsFailure)
        {
            return Result<StoreParcelResult, ParcelOperationError>.Failure(
                ParcelOperationError.PickupAccessInvalid);
        }

        var assignment = await _lockers.AssignAsync(parcel.Id, parcel.Size, cancellationToken);

        if (assignment.IsFailure)
        {
            var error = assignment.Error switch
            {
                LockerOperationError.NoCompatibleCompartment => ParcelOperationError.NoCompatibleCompartment,
                LockerOperationError.Jammed => ParcelOperationError.LockerJammed,
                LockerOperationError.Unavailable => ParcelOperationError.LockerUnavailable,
                _ => ParcelOperationError.CompartmentUnavailable
            };

            return Result<StoreParcelResult, ParcelOperationError>.Failure(error);
        }

        parcel.LockerCode = assignment.Value.LockerCode;
        parcel.CompartmentNumber = assignment.Value.Number;
        parcel.StoredAt = now;
        parcel.Status = ParcelStatus.Stored;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var messageStatus = await _pickup.SendReadyMessageAsync(
            parcel,
            preparation.Value.Code,
            cancellationToken);

        var result = new StoreParcelResult(
            assignment.Value.LockerCode,
            assignment.Value.Number,
            messageStatus);

        return Result<StoreParcelResult, ParcelOperationError>.Success(result);
    }

    private static ParcelDetails ToDetails(Parcel parcel)
    {
        return new ParcelDetails(
            parcel.Id,
            parcel.TrackingCode,
            parcel.RecipientPhone,
            parcel.Size,
            parcel.Status,
            parcel.LockerCode,
            parcel.CompartmentNumber,
            parcel.CreatedAt,
            parcel.StoredAt,
            parcel.PickedUpAt);
    }
}
