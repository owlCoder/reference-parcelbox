using ParcelBox.Domain.Common.Enums;

namespace ParcelBox.Application.DTOs.Parcels;

public sealed record RegisterParcelInput(
    string TrackingCode,
    string RecipientPhone,
    SizeCategory Size);
