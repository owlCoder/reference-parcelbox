using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Parcels;

public sealed record ParcelDetails(
    Guid Id,
    string TrackingCode,
    string RecipientPhone,
    ParcelSize Size,
    ParcelStatus Status,
    string? LockerCode,
    string? CompartmentNumber,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StoredAt,
    DateTimeOffset? PickedUpAt);
