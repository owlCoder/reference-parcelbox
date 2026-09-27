using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Parcels;

internal static class ParcelMappings
{
    public static ParcelDetails ToDetails(this Parcel parcel)
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
