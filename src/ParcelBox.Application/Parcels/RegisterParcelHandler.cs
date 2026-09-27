using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Parcels;

public sealed class RegisterParcelHandler
{
    private readonly IParcelRepository _parcels;
    private readonly IAppDbSession _db;
    private readonly TimeProvider _timeProvider;

    public RegisterParcelHandler(
        IParcelRepository parcels,
        IAppDbSession db,
        TimeProvider timeProvider)
    {
        _parcels = parcels;
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<Result<ParcelDetails, ParcelOperationError>> HandleAsync(
        RegisterParcelCommand command,
        CancellationToken cancellationToken)
    {
        var parcelResult = Parcel.Register(
            command.TrackingCode,
            command.RecipientPhone,
            command.Size,
            _timeProvider.GetUtcNow());

        if (parcelResult.IsFailure)
        {
            return Result<ParcelDetails, ParcelOperationError>.Failure(
                MapParcelError(parcelResult.Error));
        }

        var parcel = parcelResult.Value;
        var trackingCodeExists = await _parcels.TrackingCodeExistsAsync(
            parcel.TrackingCode,
            cancellationToken);

        if (trackingCodeExists)
        {
            return Result<ParcelDetails, ParcelOperationError>.Failure(
                ParcelOperationError.TrackingCodeAlreadyExists);
        }

        await _parcels.AddAsync(parcel, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<ParcelDetails, ParcelOperationError>.Success(ParcelDetails.From(parcel));
    }

    private static ParcelOperationError MapParcelError(ParcelError error)
    {
        return error switch
        {
            ParcelError.TrackingCodeRequired => ParcelOperationError.TrackingCodeRequired,
            ParcelError.RecipientPhoneRequired => ParcelOperationError.RecipientPhoneRequired,
            ParcelError.InvalidSize => ParcelOperationError.InvalidSize,
            _ => ParcelOperationError.UnexpectedDomainFailure
        };
    }
}
