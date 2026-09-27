namespace ParcelBox.Application.Parcels;

public enum ParcelOperationError
{
    None = 0,
    TrackingCodeRequired = 1,
    RecipientPhoneRequired = 2,
    InvalidSize = 3,
    TrackingCodeAlreadyExists = 4,
    NotFound = 5,
    NotRegisteredForStorage = 6,
    NoCompatibleCompartment = 7,
    LockerJammed = 8,
    LockerUnavailable = 9,
    StorageLocationRequired = 10,
    CompartmentUnavailable = 11,
    PickupAccessInvalid = 12,
    UnexpectedDomainFailure = 13
}
