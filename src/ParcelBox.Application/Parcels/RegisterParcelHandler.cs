using ParcelBox.Application.Abstractions;
using ParcelBox.Application.Common;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Parcels;

public sealed class RegisterParcelHandler(IParcelRepository parcels, IAppDbSession db)
{
    public async Task<OperationResult<ParcelDetails>> HandleAsync(
        RegisterParcelCommand command,
        CancellationToken cancellationToken)
    {
        if (await parcels.TrackingCodeExistsAsync(command.TrackingCode, cancellationToken))
            return OperationResult<ParcelDetails>.Failure("Tracking code already exists.");

        var parcel = Parcel.Register(
            command.TrackingCode,
            command.RecipientPhone,
            command.Size,
            DateTimeOffset.UtcNow);

        await parcels.AddAsync(parcel, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return OperationResult<ParcelDetails>.Success(parcel.ToDetails());
    }
}
