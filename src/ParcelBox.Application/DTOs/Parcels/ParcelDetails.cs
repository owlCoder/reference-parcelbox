using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Parcels.Enums;

namespace ParcelBox.Application.DTOs.Parcels;

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
