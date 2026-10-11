using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Enums;

namespace ParcelBox.Application.DTOs.Lockers;

public sealed class CompartmentDetails
{
    public Guid Id { get; set; } = Guid.Empty;
    public string LockerCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public SizeCategory Size { get; set; };
    public CompartmentStatus Status { get; set; };
    public Guid? ParcelId { get; set; };

    public CompartmentDetails()
    {
    }

    public CompartmentDetails(Guid id, string lockerCode, string number, SizeCategory size, CompartmentStatus status, Guid? parcelId)
    {
        Id = id;
        LockerCode = lockerCode;
        Number = number;
        Size = size;
        Status = status;
        ParcelId = parcelId;
    }
}
