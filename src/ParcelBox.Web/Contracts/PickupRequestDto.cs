namespace ParcelBox.Web.Contracts;

public sealed class PickupRequestDto
{
    public string Code { get; set; } = string.Empty;

    public PickupRequestDto()
    {
    }

    public PickupRequestDto(string code)
    {
        Code = code;
    }
}
