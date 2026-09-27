using ParcelBox.Domain.Parcels;

namespace ParcelBox.Api.Contracts;

public sealed record RegisterParcelRequest(
    string TrackingCode,
    string RecipientPhone,
    ParcelSize Size);
