using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Domain.Pickup;

public static class PickupAccessErrors
{
    public static Error ParcelIdRequired { get; } = new(
        "PickupAccess.ParcelIdRequired",
        "Parcel id is required.",
        ErrorType.Validation);

    public static Error CodeHashRequired { get; } = new(
        "PickupAccess.CodeHashRequired",
        "Pickup code hash is required.",
        ErrorType.Validation);

    public static Error ExpirationRequired { get; } = new(
        "PickupAccess.ExpirationRequired",
        "Pickup expiration is required.",
        ErrorType.Validation);

    public static Error NotActive { get; } = new(
        "PickupAccess.NotActive",
        "Pickup access is not active.",
        ErrorType.Conflict);

    public static Error Expired { get; } = new(
        "PickupAccess.Expired",
        "Pickup access has expired.",
        ErrorType.Conflict);
}
