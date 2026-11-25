using ParcelBox.Web.Enums;

namespace ParcelBox.Web.Contracts;

public sealed class StoreParcelResultDto
{
    public string LockerCode { get; set; } = string.Empty;
    public string CompartmentNumber { get; set; } = string.Empty;
    public PickupMessageStatus MessageDelivery { get; set; }
    public CompartmentSelectionStrategy Strategy { get; set; }
    public string SelectionReason { get; set; } = string.Empty;

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
