using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Lockers;

internal static class CompartmentSizeMapping
{
    public static Result<CompartmentSize> ToCompartmentSize(ParcelSize size)
    {
        switch (size)
        {
            case ParcelSize.Small:
                return Result<CompartmentSize>.Success(CompartmentSize.Small);
            case ParcelSize.Medium:
                return Result<CompartmentSize>.Success(CompartmentSize.Medium);
            case ParcelSize.Large:
                return Result<CompartmentSize>.Success(CompartmentSize.Large);
            default:
                return Result<CompartmentSize>.Failure(ParcelErrors.InvalidSize);
        }
    }
}
