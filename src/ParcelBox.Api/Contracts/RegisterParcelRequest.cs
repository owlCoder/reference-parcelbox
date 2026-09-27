using ParcelBox.Domain.Common.Enums;

namespace ParcelBox.Api.Contracts;

public sealed record RegisterParcelRequest(
    string TrackingCode,
    string RecipientPhone,
    SizeCategory Size);
