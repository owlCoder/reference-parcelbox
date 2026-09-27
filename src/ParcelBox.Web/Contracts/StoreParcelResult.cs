namespace ParcelBox.Web.Contracts;

public sealed record StoreParcelResult(
    string LockerCode,
    string CompartmentNumber,
    PickupMessageStatus MessageDelivery);
