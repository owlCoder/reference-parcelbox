namespace ParcelBox.Web.Contracts;

public sealed record ParcelDetails(
    Guid Id,
    string TrackingCode,
    string RecipientPhone,
    SizeCategory Size,
    ParcelStatus Status,
    string? LockerCode,
    string? CompartmentNumber,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StoredAt,
    DateTimeOffset? PickedUpAt);
