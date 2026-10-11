namespace ParcelBox.Application.DTOs.Pickup;

public sealed class PickupAccessPreparation
{
    public string Code { get; set; } = string.Empty;

    public PickupAccessPreparation()
    {
    }

    public PickupAccessPreparation(string code)
    {
        Code = code;
    }
}
