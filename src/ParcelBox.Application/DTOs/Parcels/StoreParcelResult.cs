using ParcelBox.Application.Enums;

namespace ParcelBox.Application.DTOs.Parcels;

public sealed record StoreParcelResult(
    string LockerCode,
    string CompartmentNumber,
    PickupMessageStatus MessageDelivery);
