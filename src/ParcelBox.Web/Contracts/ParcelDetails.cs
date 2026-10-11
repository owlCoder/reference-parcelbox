using ParcelBox.Web.Enums;

namespace ParcelBox.Web.Contracts;

public sealed class ParcelDetails
{
    public Guid Id { get; set; } = Guid.Empty;
    public string TrackingCode { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public SizeCategory Size { get; set; }
    public ParcelStatus Status { get; set; }
    public string? LockerCode { get; set; } = string.Empty;
    public string? CompartmentNumber { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? StoredAt { get; set; }
    public DateTimeOffset? PickedUpAt { get; set; }

    public ParcelDetails()
    {
    }

    public ParcelDetails(Guid id, string trackingCode, string recipientPhone, SizeCategory size, ParcelStatus status, string? lockerCode, string? compartmentNumber, DateTimeOffset createdAt, DateTimeOffset? storedAt, DateTimeOffset? pickedUpAt)
    {
        Id = id;
        TrackingCode = trackingCode;
        RecipientPhone = recipientPhone;
        Size = size;
        Status = status;
        LockerCode = lockerCode;
        CompartmentNumber = compartmentNumber;
        CreatedAt = createdAt;
        StoredAt = storedAt;
        PickedUpAt = pickedUpAt;
    }
}
