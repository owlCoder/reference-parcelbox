using ParcelBox.Domain.Common;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Domain.Lockers;

public sealed class Compartment
{
    private Compartment() { }

    public Compartment(Guid id, string lockerCode, string number, ParcelSize size)
    {
        Id = id;
        LockerCode = lockerCode;
        Number = number;
        Size = size;
        Status = CompartmentStatus.Available;
    }

    public Guid Id { get; private set; }
    public string LockerCode { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public ParcelSize Size { get; private set; }
    public CompartmentStatus Status { get; private set; }
    public Guid? ParcelId { get; private set; }

    public bool CanFit(ParcelSize parcelSize) =>
        Status == CompartmentStatus.Available && (int)Size >= (int)parcelSize;

    public void Occupy(Guid parcelId)
    {
        if (Status != CompartmentStatus.Available)
            throw new DomainException("Compartment is not available.");

        Status = CompartmentStatus.Occupied;
        ParcelId = parcelId;
    }

    public void Release(Guid parcelId)
    {
        if (Status != CompartmentStatus.Occupied || ParcelId != parcelId)
            throw new DomainException("Compartment is not occupied by this parcel.");

        Status = CompartmentStatus.Available;
        ParcelId = null;
    }
}
