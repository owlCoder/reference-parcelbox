namespace ParcelBox.Web.Contracts;

public sealed record CompartmentDetails(
    Guid Id,
    string LockerCode,
    string Number,
    SizeCategory Size,
    CompartmentStatus Status,
    Guid? ParcelId);
