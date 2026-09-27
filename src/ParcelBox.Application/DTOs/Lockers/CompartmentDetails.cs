using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Enums;

namespace ParcelBox.Application.DTOs.Lockers;

public sealed record CompartmentDetails(
    Guid Id,
    string LockerCode,
    string Number,
    SizeCategory Size,
    CompartmentStatus Status,
    Guid? ParcelId);
