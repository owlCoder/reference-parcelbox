using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Parcels;

public sealed class GetParcelHandler
{
    private readonly IParcelRepository _parcels;

    public GetParcelHandler(IParcelRepository parcels)
    {
        _parcels = parcels;
    }

    public async Task<Result<ParcelDetails, ParcelOperationError>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var parcel = await _parcels.GetByIdAsync(id, cancellationToken);

        if (parcel is null)
        {
            return Result<ParcelDetails, ParcelOperationError>.Failure(ParcelOperationError.NotFound);
        }

        return Result<ParcelDetails, ParcelOperationError>.Success(ParcelDetails.From(parcel));
    }
}
