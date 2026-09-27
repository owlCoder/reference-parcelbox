using ParcelBox.Application.Enums;

namespace ParcelBox.Api.Extensions;

internal static class PickupErrorHttpExtensions
{
    public static IResult ToProblemResult(this PickupOperationError error)
    {
        return error switch
        {
            PickupOperationError.TrackingCodeRequired => ProblemResults.Create(error, 400, "Tracking code is required."),
            PickupOperationError.PickupCodeRequired => ProblemResults.Create(error, 400, "Pickup code is required."),
            PickupOperationError.ParcelNotFound => ProblemResults.Create(error, 404, "Parcel was not found."),
            PickupOperationError.ParcelNotStored => ProblemResults.Create(error, 409, "Parcel is not ready for pickup."),
            PickupOperationError.PickupAccessNotFound => ProblemResults.Create(error, 404, "Pickup access was not found."),
            PickupOperationError.PickupCodeInvalid => ProblemResults.Create(error, 400, "Pickup code is invalid."),
            PickupOperationError.PickupCodeExpired => ProblemResults.Create(error, 409, "Pickup code has expired."),
            PickupOperationError.PickupCodeLocked => ProblemResults.Create(error, 409, "Pickup access is locked."),
            PickupOperationError.PickupCodeUsed => ProblemResults.Create(error, 409, "Pickup code has already been used."),
            PickupOperationError.CompartmentNotFound => ProblemResults.Create(error, 404, "Compartment was not found."),
            PickupOperationError.LockerJammed => ProblemResults.Create(error, 503, "Locker compartment is jammed."),
            PickupOperationError.LockerUnavailable => ProblemResults.Create(error, 503, "Locker controller is unavailable."),
            PickupOperationError.CompartmentStateConflict => ProblemResults.Create(error, 409, "Compartment state changed during pickup."),
            _ => ProblemResults.Create(error, 500, "The operation could not be completed.")
        };
    }
}
