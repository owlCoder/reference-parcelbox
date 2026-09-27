namespace ParcelBox.Domain.Pickup;

public enum PickupAccessError
{
    None = 0,
    ParcelIdRequired = 1,
    CodeHashRequired = 2,
    ExpirationRequired = 3,
    NotActive = 4,
    Expired = 5
}
