using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Domain.Lockers;

public sealed class Compartment
{
    private Compartment()
    {
    }

    private Compartment(Guid id, string lockerCode, string number, SizeCategory size)
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
    public SizeCategory Size { get; private set; }
    public CompartmentStatus Status { get; private set; }
    public Guid? ParcelId { get; private set; }

    public static Result<Compartment, CompartmentError> Create(
        string lockerCode,
        string number,
        SizeCategory size)
    {
        if (string.IsNullOrWhiteSpace(lockerCode))
        {
            return Result<Compartment, CompartmentError>.Failure(CompartmentError.LockerCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            return Result<Compartment, CompartmentError>.Failure(CompartmentError.NumberRequired);
        }

        if (!Enum.IsDefined(size))
        {
            return Result<Compartment, CompartmentError>.Failure(CompartmentError.InvalidSize);
        }

        var compartment = new Compartment(
            Guid.NewGuid(),
            lockerCode.Trim(),
            number.Trim(),
            size);

        return Result<Compartment, CompartmentError>.Success(compartment);
    }

    public bool CanFit(SizeCategory requiredSize)
    {
        if (Status != CompartmentStatus.Available || !Enum.IsDefined(requiredSize))
        {
            return false;
        }

        return Size switch
        {
            SizeCategory.Small => requiredSize == SizeCategory.Small,
            SizeCategory.Medium => requiredSize is SizeCategory.Small or SizeCategory.Medium,
            SizeCategory.Large => true,
            _ => false
        };
    }

    public Result<CompartmentError> Occupy(Guid parcelId)
    {
        if (parcelId == Guid.Empty)
        {
            return Result<CompartmentError>.Failure(CompartmentError.ParcelIdRequired);
        }

        if (Status != CompartmentStatus.Available)
        {
            return Result<CompartmentError>.Failure(CompartmentError.NotAvailable);
        }

        Status = CompartmentStatus.Occupied;
        ParcelId = parcelId;

        return Result<CompartmentError>.Success();
    }

    public Result<CompartmentError> Release(Guid parcelId)
    {
        if (Status != CompartmentStatus.Occupied || ParcelId != parcelId)
        {
            return Result<CompartmentError>.Failure(CompartmentError.NotOccupiedByParcel);
        }

        Status = CompartmentStatus.Available;
        ParcelId = null;

        return Result<CompartmentError>.Success();
    }
}
