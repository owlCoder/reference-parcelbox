namespace ParcelBox.Web.Contracts;

public sealed class PickupRequest
{
    public string Code { get; set; } = string.Empty;

    public PickupRequest()
    {
    }

    public PickupRequest(string code)
    {
        Code = code;
    }
}
