using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Lockers;

internal static class CompartmentSizeMapping
{
    public static CompartmentSize ToCompartmentSize(this ParcelSize size) => size switch
    {
        ParcelSize.Small => CompartmentSize.Small,
        ParcelSize.Medium => CompartmentSize.Medium,
        ParcelSize.Large => CompartmentSize.Large,
        _ => throw new ArgumentOutOfRangeException(nameof(size), size, "Unsupported parcel size.")
    };
}
