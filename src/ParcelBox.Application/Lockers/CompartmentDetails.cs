using ParcelBox.Domain.Lockers;

namespace ParcelBox.Application.Lockers;

public sealed record CompartmentDetails(
    Guid Id,
    string LockerCode,
    string Number,
    CompartmentSize Size,
    CompartmentStatus Status,
    Guid? ParcelId);
