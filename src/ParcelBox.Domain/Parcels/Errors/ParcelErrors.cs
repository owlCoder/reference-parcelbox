using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Domain.Parcels;

public static class ParcelErrors
{
    public static Error TrackingCodeRequired { get; } = new(
        "Parcel.TrackingCodeRequired",
        "Tracking code is required.",
        ErrorType.Validation);

    public static Error RecipientPhoneRequired { get; } = new(
        "Parcel.RecipientPhoneRequired",
        "Recipient phone is required.",
        ErrorType.Validation);

    public static Error InvalidSize { get; } = new(
        "Parcel.InvalidSize",
        "Parcel size is invalid.",
        ErrorType.Validation);

    public static Error NotRegisteredForStorage { get; } = new(
        "Parcel.NotRegisteredForStorage",
        "Only a registered parcel can be stored.",
        ErrorType.Conflict);

    public static Error StorageLocationRequired { get; } = new(
        "Parcel.StorageLocationRequired",
        "Locker and compartment are required.",
        ErrorType.Validation);

    public static Error NotStoredForPickup { get; } = new(
        "Parcel.NotStoredForPickup",
        "Only a stored parcel can be picked up.",
        ErrorType.Conflict);

    public static Error NotRegisteredForCancellation { get; } = new(
        "Parcel.NotRegisteredForCancellation",
        "Only a registered parcel can be cancelled.",
        ErrorType.Conflict);
}
