using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Application.Lockers;

public sealed record CompartmentDetails(
    Guid Id,
    string LockerCode,
    string Number,
    SizeCategory Size,
    CompartmentStatus Status,
    Guid? ParcelId)
{
    public static CompartmentDetails From(Compartment compartment)
    {
        return new CompartmentDetails(
            compartment.Id,
            compartment.LockerCode,
            compartment.Number,
            compartment.Size,
            compartment.Status,
            compartment.ParcelId);
    }
}
