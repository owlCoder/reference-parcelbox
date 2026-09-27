using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Pickup;

public static class PickupApplicationErrors
{
    public static Error TrackingCodeRequired { get; } = new(
        "Pickup.TrackingCodeRequired",
        "Tracking code is required.",
        ErrorType.Validation);

    public static Error PickupCodeRequired { get; } = new(
        "Pickup.CodeRequired",
        "Pickup code is required.",
        ErrorType.Validation);

    public static Error AccessNotFound { get; } = new(
        "Pickup.AccessNotFound",
        "Pickup access was not found.",
        ErrorType.NotFound);

    public static Error CompartmentNotFound { get; } = new(
        "Pickup.CompartmentNotFound",
        "Compartment was not found.",
        ErrorType.NotFound);

    public static Error FromValidation(PickupCodeValidation validation)
    {
        switch (validation)
        {
            case PickupCodeValidation.Expired:
                return new Error("Pickup.CodeExpired", "Pickup code has expired.", ErrorType.Conflict);
            case PickupCodeValidation.Locked:
                return new Error("Pickup.CodeLocked", "Pickup access is locked.", ErrorType.Conflict);
            case PickupCodeValidation.Used:
                return new Error("Pickup.CodeUsed", "Pickup code has already been used.", ErrorType.Conflict);
            default:
                return new Error("Pickup.CodeInvalid", "Pickup code is invalid.", ErrorType.Validation);
        }
    }
}
