namespace ParcelBox.Domain.Pickup;

public enum PickupCodeValidation
{
    Valid = 1,
    Invalid = 2,
    Expired = 3,
    Locked = 4,
    Used = 5
}
