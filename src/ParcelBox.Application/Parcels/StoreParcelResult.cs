using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Parcels;

public sealed record StoreParcelResult(
    string LockerCode,
    string CompartmentNumber,
    MessageDeliveryStatus MessageDelivery);
