using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Parcels;

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
    DateTimeOffset? PickedUpAt)
{
    public static ParcelDetails From(Parcel parcel)
    {
        return new ParcelDetails(
            parcel.Id,
            parcel.TrackingCode,
            parcel.RecipientPhone,
            parcel.Size,
            parcel.Status,
            parcel.LockerCode,
            parcel.CompartmentNumber,
            parcel.CreatedAt,
            parcel.StoredAt,
            parcel.PickedUpAt);
    }
}
