using ParcelBox.Domain.Common;

namespace ParcelBox.Domain.Lockers;

public sealed class Compartment
{
    private Compartment() { }

    private Compartment(Guid id, string lockerCode, string number, CompartmentSize size)
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
    public CompartmentSize Size { get; private set; }
    public CompartmentStatus Status { get; private set; }
    public Guid? ParcelId { get; private set; }

    public static Compartment Create(string lockerCode, string number, CompartmentSize size)
    {
        if (string.IsNullOrWhiteSpace(lockerCode))
            throw new DomainException("Locker code is required.");

        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Compartment number is required.");

        if (!Enum.IsDefined(size))
            throw new DomainException("Compartment size is invalid.");

        return new Compartment(Guid.NewGuid(), lockerCode.Trim(), number.Trim(), size);
    }

    public bool CanFit(CompartmentSize requiredSize) =>
        Status == CompartmentStatus.Available && (int)Size >= (int)requiredSize;

    public void Occupy(Guid parcelId)
    {
        if (parcelId == Guid.Empty)
            throw new DomainException("Parcel id is required.");

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
