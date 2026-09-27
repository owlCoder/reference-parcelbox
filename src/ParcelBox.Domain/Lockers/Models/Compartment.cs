using ParcelBox.Domain.Common.Enums;

namespace ParcelBox.Domain.Lockers;

public sealed class Compartment
{
    public Guid Id { get; set; }
    public string LockerCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public SizeCategory Size { get; set; }
    public CompartmentStatus Status { get; set; }
    public Guid? ParcelId { get; set; }
}
