namespace ParcelBox.Application.Enums;

public enum LockerOperationError
{
    None = 0,
    InvalidParcel = 1,
    InvalidSize = 2,
    NoCompatibleCompartment = 3,
    CompartmentNotFound = 4,
    CompartmentUnavailable = 5,
    CompartmentStateConflict = 6,
    Jammed = 7,
    Unavailable = 8
}
