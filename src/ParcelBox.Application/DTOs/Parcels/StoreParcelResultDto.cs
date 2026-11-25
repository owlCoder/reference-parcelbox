using ParcelBox.Application.Enums;

namespace ParcelBox.Application.DTOs.Parcels;

public sealed class StoreParcelResultDto
{
    public string LockerCode { get; set; } = string.Empty;
    public string CompartmentNumber { get; set; } = string.Empty;
    public PickupMessageStatus MessageDelivery { get; set; }
    public CompartmentSelectionStrategy Strategy { get; set; }
    public string SelectionReason { get; set; } = string.Empty;

    public StoreParcelResultDto(string lockerCode, string compartmentNumber, PickupMessageStatus messageDelivery, CompartmentSelectionStrategy strategy, string selectionReason)
    {
        LockerCode = lockerCode;
        CompartmentNumber = compartmentNumber;
        MessageDelivery = messageDelivery;
        Strategy = strategy;
        SelectionReason = selectionReason;
    }

    public StoreParcelResultDto()
    {
    }

    public StoreParcelResultDto(string lockerCode, string compartmentNumber, PickupMessageStatus messageDelivery)
    {
        LockerCode = lockerCode;
        CompartmentNumber = compartmentNumber;
        MessageDelivery = messageDelivery;
    }
}
