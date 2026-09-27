namespace ParcelBox.Domain.Lockers;

public enum CompartmentError
{
    None = 0,
    LockerCodeRequired = 1,
    NumberRequired = 2,
    InvalidSize = 3,
    ParcelIdRequired = 4,
    NotAvailable = 5,
    NotOccupiedByParcel = 6
}
