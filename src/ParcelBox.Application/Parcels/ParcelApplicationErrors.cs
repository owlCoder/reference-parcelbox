using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Parcels;

public static class ParcelApplicationErrors
{
    public static Error NotFound { get; } = new(
        "Parcel.NotFound",
        "Parcel was not found.",
        ErrorType.NotFound);

    public static Error TrackingCodeAlreadyExists { get; } = new(
        "Parcel.TrackingCodeAlreadyExists",
        "Tracking code already exists.",
        ErrorType.Conflict);

    public static Error NoCompatibleCompartment { get; } = new(
        "Parcel.NoCompatibleCompartment",
        "No compatible compartment is available.",
        ErrorType.Conflict);
}
