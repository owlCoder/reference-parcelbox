using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Domain.Lockers;

public sealed class Compartment
{
    private Compartment()
    {
    }

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

    public static Result<Compartment> Create(string lockerCode, string number, CompartmentSize size)
    {
        if (string.IsNullOrWhiteSpace(lockerCode))
        {
            return Result<Compartment>.Failure(CompartmentErrors.LockerCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            return Result<Compartment>.Failure(CompartmentErrors.NumberRequired);
        }

        if (!Enum.IsDefined(size))
        {
            return Result<Compartment>.Failure(CompartmentErrors.InvalidSize);
        }

        var compartment = new Compartment(
            Guid.NewGuid(),
            lockerCode.Trim(),
            number.Trim(),
            size);

        return Result<Compartment>.Success(compartment);
    }

    public bool CanFit(CompartmentSize requiredSize)
    {
        return Status == CompartmentStatus.Available && (int)Size >= (int)requiredSize;
    }

    public Result Occupy(Guid parcelId)
    {
        if (parcelId == Guid.Empty)
        {
            return Result.Failure(CompartmentErrors.ParcelIdRequired);
        }

        if (Status != CompartmentStatus.Available)
        {
            return Result.Failure(CompartmentErrors.NotAvailable);
        }

        Status = CompartmentStatus.Occupied;
        ParcelId = parcelId;

        return Result.Success();
    }

    public Result Release(Guid parcelId)
    {
        if (Status != CompartmentStatus.Occupied || ParcelId != parcelId)
        {
            return Result.Failure(CompartmentErrors.NotOccupiedByParcel);
        }

        Status = CompartmentStatus.Available;
        ParcelId = null;

        return Result.Success();
    }
}
