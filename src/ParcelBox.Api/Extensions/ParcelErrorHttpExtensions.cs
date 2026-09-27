using ParcelBox.Application.Enums;

namespace ParcelBox.Api.Extensions;

internal static class ParcelErrorHttpExtensions
{
    public static IResult ToProblemResult(this ParcelOperationError error)
    {
        return error switch
        {
            ParcelOperationError.TrackingCodeRequired => ProblemResults.Create(error, 400, "Tracking code is required."),
            ParcelOperationError.RecipientPhoneRequired => ProblemResults.Create(error, 400, "Recipient phone is required."),
            ParcelOperationError.InvalidSize => ProblemResults.Create(error, 400, "Parcel size is invalid."),
            ParcelOperationError.TrackingCodeAlreadyExists => ProblemResults.Create(error, 409, "Tracking code already exists."),
            ParcelOperationError.NotFound => ProblemResults.Create(error, 404, "Parcel was not found."),
            ParcelOperationError.NotRegisteredForStorage => ProblemResults.Create(error, 409, "Only a registered parcel can be stored."),
            ParcelOperationError.NoCompatibleCompartment => ProblemResults.Create(error, 409, "No compatible compartment is available."),
            ParcelOperationError.LockerJammed => ProblemResults.Create(error, 503, "Locker compartment is jammed."),
            ParcelOperationError.LockerUnavailable => ProblemResults.Create(error, 503, "Locker controller is unavailable."),
            ParcelOperationError.CompartmentUnavailable => ProblemResults.Create(error, 409, "Compartment is no longer available."),
            ParcelOperationError.PickupAccessInvalid => ProblemResults.Create(error, 500, "Pickup access could not be prepared."),
            _ => ProblemResults.Create(error, 500, "The operation could not be completed.")
        };
    }
}
