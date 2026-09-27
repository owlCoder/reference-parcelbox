using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Parcels;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Persistence;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Parcels.Enums;
using ParcelBox.Domain.Parcels.Models;

namespace ParcelBox.Application.Services;

public sealed class ParcelService : IParcelService
{
    private readonly IParcelRepository _parcels;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ParcelService(
        IParcelRepository parcels,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _parcels = parcels;
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
