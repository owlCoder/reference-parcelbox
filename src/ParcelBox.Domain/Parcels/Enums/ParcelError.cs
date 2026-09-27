namespace ParcelBox.Domain.Parcels;

public enum ParcelError
{
    None = 0,
    TrackingCodeRequired = 1,
    RecipientPhoneRequired = 2,
    InvalidSize = 3,
    NotRegisteredForStorage = 4,
    StorageLocationRequired = 5,
    NotStoredForPickup = 6
}
