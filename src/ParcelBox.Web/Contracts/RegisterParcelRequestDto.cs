using ParcelBox.Web.Enums;

namespace ParcelBox.Web.Contracts;

public sealed class RegisterParcelRequestDto
{
    public string TrackingCode { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public SizeCategory Size { get; set; }

    public RegisterParcelRequestDto()
    {
    }

    public RegisterParcelRequestDto(string trackingCode, string recipientPhone, SizeCategory size)
    {
        TrackingCode = trackingCode;
        RecipientPhone = recipientPhone;
        Size = size;
    }
}
