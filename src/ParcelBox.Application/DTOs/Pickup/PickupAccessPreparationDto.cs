namespace ParcelBox.Application.DTOs.Pickup;

public sealed class PickupAccessPreparationDto
{
    public string Code { get; set; } = string.Empty;

    public PickupAccessPreparationDto()
    {
    }

    public PickupAccessPreparationDto(string code)
    {
        Code = code;
    }
}
