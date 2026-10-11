using ParcelBox.Domain.Common.Enums;

namespace ParcelBox.Application.DTOs.Parcels;

public sealed class RegisterParcelInputDto
{
    public string TrackingCode { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public SizeCategory Size { get; set; }

    public RegisterParcelInputDto()
    {
    }

    public RegisterParcelInputDto(string trackingCode, string recipientPhone, SizeCategory size)
    {
        TrackingCode = trackingCode;
        RecipientPhone = recipientPhone;
        Size = size;
    }
}
