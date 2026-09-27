using ParcelBox.Application.Enums;

namespace ParcelBox.Api.Extensions;

internal static class ErrorHttpExtensions
{
    public static IResult ToProblemResult(this ParcelOperationError error)
    {
        return error switch
        {
            ParcelOperationError.TrackingCodeRequired => Problem(error, 400, "Tracking code is required."),
            ParcelOperationError.RecipientPhoneRequired => Problem(error, 400, "Recipient phone is required."),
            ParcelOperationError.InvalidSize => Problem(error, 400, "Parcel size is invalid."),
            ParcelOperationError.TrackingCodeAlreadyExists => Problem(error, 409, "Tracking code already exists."),
            ParcelOperationError.NotFound => Problem(error, 404, "Parcel was not found."),
            ParcelOperationError.NotRegisteredForStorage => Problem(error, 409, "Only a registered parcel can be stored."),
            ParcelOperationError.NoCompatibleCompartment => Problem(error, 409, "No compatible compartment is available."),
            ParcelOperationError.LockerJammed => Problem(error, 503, "Locker compartment is jammed."),
            ParcelOperationError.LockerUnavailable => Problem(error, 503, "Locker controller is unavailable."),
            ParcelOperationError.CompartmentUnavailable => Problem(error, 409, "Compartment is no longer available."),
            ParcelOperationError.PickupAccessInvalid => Problem(error, 500, "Pickup access could not be prepared."),
            _ => Problem(error, 500, "The operation could not be completed.")
        };
    }

    public static IResult ToProblemResult(this PickupOperationError error)
    {
        return error switch
        {
            PickupOperationError.TrackingCodeRequired => Problem(error, 400, "Tracking code is required."),
            PickupOperationError.PickupCodeRequired => Problem(error, 400, "Pickup code is required."),
            PickupOperationError.ParcelNotFound => Problem(error, 404, "Parcel was not found."),
            PickupOperationError.ParcelNotStored => Problem(error, 409, "Parcel is not ready for pickup."),
            PickupOperationError.PickupAccessNotFound => Problem(error, 404, "Pickup access was not found."),
            PickupOperationError.PickupCodeInvalid => Problem(error, 400, "Pickup code is invalid."),
            PickupOperationError.PickupCodeExpired => Problem(error, 409, "Pickup code has expired."),
            PickupOperationError.PickupCodeLocked => Problem(error, 409, "Pickup access is locked."),
            PickupOperationError.PickupCodeUsed => Problem(error, 409, "Pickup code has already been used."),
            PickupOperationError.CompartmentNotFound => Problem(error, 404, "Compartment was not found."),
            PickupOperationError.LockerJammed => Problem(error, 503, "Locker compartment is jammed."),
            PickupOperationError.LockerUnavailable => Problem(error, 503, "Locker controller is unavailable."),
            PickupOperationError.CompartmentStateConflict => Problem(error, 409, "Compartment state changed during pickup."),
            _ => Problem(error, 500, "The operation could not be completed.")
        };
    }

    private static IResult Problem<TError>(TError error, int statusCode, string detail)
        where TError : struct, Enum
    {
        return Results.Problem(
            statusCode: statusCode,
            title: error.ToString(),
            detail: detail);
    }
}
