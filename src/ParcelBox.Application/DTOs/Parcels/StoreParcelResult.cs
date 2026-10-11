using ParcelBox.Application.Enums;

namespace ParcelBox.Application.DTOs.Parcels;

public sealed class StoreParcelResult
{
    public string LockerCode { get; set; } = string.Empty;
    public string CompartmentNumber { get; set; } = string.Empty;
    public PickupMessageStatus MessageDelivery { get; set; }

    public StoreParcelResult()
    {
    }

    public StoreParcelResult(string lockerCode, string compartmentNumber, PickupMessageStatus messageDelivery)
    {
        LockerCode = lockerCode;
        CompartmentNumber = compartmentNumber;
        MessageDelivery = messageDelivery;
    }
}
