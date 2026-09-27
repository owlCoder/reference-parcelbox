using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Lockers;

public sealed record CompartmentDetails(
    Guid Id,
    string LockerCode,
    string Number,
    ParcelSize Size,
    CompartmentStatus Status,
    Guid? ParcelId);
