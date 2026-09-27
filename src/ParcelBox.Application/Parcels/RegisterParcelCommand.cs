using ParcelBox.Domain.Common.Enums;

namespace ParcelBox.Application.Parcels;

public sealed record RegisterParcelCommand(
    string TrackingCode,
    string RecipientPhone,
    SizeCategory Size);
