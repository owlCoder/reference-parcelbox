using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Parcels;

public sealed record RegisterParcelCommand(
    string TrackingCode,
    string RecipientPhone,
    ParcelSize Size);
