using ParcelBox.Domain.Common.Enums;

namespace ParcelBox.Domain.Parcels;

public sealed class Parcel
{
    public Guid Id { get; set; }
    public string TrackingCode { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public SizeCategory Size { get; set; }
    public ParcelStatus Status { get; set; }
    public string? LockerCode { get; set; }
    public string? CompartmentNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? StoredAt { get; set; }
    public DateTimeOffset? PickedUpAt { get; set; }
}
