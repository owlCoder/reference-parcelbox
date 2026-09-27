namespace ParcelBox.Application.Parcels;

public sealed record StoreParcelResult(
    string LockerCode,
    string CompartmentNumber,
    PickupMessageStatus MessageDelivery);
