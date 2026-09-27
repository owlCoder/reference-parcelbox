namespace ParcelBox.Web.Contracts;

public sealed record RegisterParcelRequest(
    string TrackingCode,
    string RecipientPhone,
    SizeCategory Size);
