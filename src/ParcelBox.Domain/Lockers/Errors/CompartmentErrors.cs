using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Domain.Lockers;

public static class CompartmentErrors
{
    public static Error LockerCodeRequired { get; } = new(
        "Compartment.LockerCodeRequired",
        "Locker code is required.",
        ErrorType.Validation);

    public static Error NumberRequired { get; } = new(
        "Compartment.NumberRequired",
        "Compartment number is required.",
        ErrorType.Validation);

    public static Error InvalidSize { get; } = new(
        "Compartment.InvalidSize",
        "Compartment size is invalid.",
        ErrorType.Validation);

    public static Error ParcelIdRequired { get; } = new(
        "Compartment.ParcelIdRequired",
        "Parcel id is required.",
        ErrorType.Validation);

    public static Error NotAvailable { get; } = new(
        "Compartment.NotAvailable",
        "Compartment is not available.",
        ErrorType.Conflict);

    public static Error NotOccupiedByParcel { get; } = new(
        "Compartment.NotOccupiedByParcel",
        "Compartment is not occupied by this parcel.",
        ErrorType.Conflict);
}
