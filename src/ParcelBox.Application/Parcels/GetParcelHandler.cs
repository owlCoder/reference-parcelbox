using ParcelBox.Application.Abstractions;
using ParcelBox.Application.Common;

namespace ParcelBox.Application.Parcels;

public sealed class GetParcelHandler(IParcelRepository parcels)
{
    public async Task<OperationResult<ParcelDetails>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var parcel = await parcels.GetByIdAsync(id, cancellationToken);
        return parcel is null
            ? OperationResult<ParcelDetails>.Failure("Parcel not found.")
            : OperationResult<ParcelDetails>.Success(parcel.ToDetails());
    }
}
