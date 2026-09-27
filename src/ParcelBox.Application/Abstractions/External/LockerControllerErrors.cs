using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Abstractions.External;

public static class LockerControllerErrors
{
    public static Error Jammed { get; } = new(
        "LockerController.Jammed",
        "Locker compartment is jammed.",
        ErrorType.Unavailable);

    public static Error Unavailable { get; } = new(
        "LockerController.Unavailable",
        "Locker controller is unavailable.",
        ErrorType.Unavailable);
}
