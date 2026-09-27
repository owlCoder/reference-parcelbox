namespace ParcelBox.Application.Pickup;

public enum PickupOperationError
{
    None = 0,
    TrackingCodeRequired = 1,
    PickupCodeRequired = 2,
    ParcelNotFound = 3,
    ParcelNotStored = 4,
    PickupAccessNotFound = 5,
    PickupCodeInvalid = 6,
    PickupCodeExpired = 7,
    PickupCodeLocked = 8,
    PickupCodeUsed = 9,
    CompartmentNotFound = 10,
    LockerJammed = 11,
    LockerUnavailable = 12,
    PickupAccessNotActive = 13,
    PickupAccessExpired = 14,
    CompartmentStateConflict = 15,
    UnexpectedDomainFailure = 16
}
