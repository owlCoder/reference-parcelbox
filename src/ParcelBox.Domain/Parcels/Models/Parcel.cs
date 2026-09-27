using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Domain.Parcels;

public sealed class Parcel
{
    private Parcel()
    {
    }

    private Parcel(
        Guid id,
        string trackingCode,
        string recipientPhone,
        SizeCategory size,
        DateTimeOffset createdAt)
    {
        Id = id;
        TrackingCode = trackingCode;
        RecipientPhone = recipientPhone;
        Size = size;
        Status = ParcelStatus.Registered;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string TrackingCode { get; private set; } = string.Empty;
    public string RecipientPhone { get; private set; } = string.Empty;
    public SizeCategory Size { get; private set; }
    public ParcelStatus Status { get; private set; }
    public string? LockerCode { get; private set; }
    public string? CompartmentNumber { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? StoredAt { get; private set; }
    public DateTimeOffset? PickedUpAt { get; private set; }

    public bool CanBeStored => Status == ParcelStatus.Registered;
    public bool CanBePickedUp => Status == ParcelStatus.Stored;

    public static Result<Parcel, ParcelError> Register(
        string trackingCode,
        string recipientPhone,
        SizeCategory size,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
        {
            return Result<Parcel, ParcelError>.Failure(ParcelError.TrackingCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(recipientPhone))
        {
            return Result<Parcel, ParcelError>.Failure(ParcelError.RecipientPhoneRequired);
        }

        if (!Enum.IsDefined(size))
        {
            return Result<Parcel, ParcelError>.Failure(ParcelError.InvalidSize);
        }

        var parcel = new Parcel(
            Guid.NewGuid(),
            trackingCode.Trim(),
            recipientPhone.Trim(),
            size,
            now);

        return Result<Parcel, ParcelError>.Success(parcel);
    }

    public Result<ParcelError> Store(
        string lockerCode,
        string compartmentNumber,
        DateTimeOffset now)
    {
        if (!CanBeStored)
        {
            return Result<ParcelError>.Failure(ParcelError.NotRegisteredForStorage);
        }

        if (string.IsNullOrWhiteSpace(lockerCode) || string.IsNullOrWhiteSpace(compartmentNumber))
        {
            return Result<ParcelError>.Failure(ParcelError.StorageLocationRequired);
        }

        LockerCode = lockerCode.Trim();
        CompartmentNumber = compartmentNumber.Trim();
        StoredAt = now;
        Status = ParcelStatus.Stored;

        return Result<ParcelError>.Success();
    }

    public Result<ParcelError> MarkPickedUp(DateTimeOffset now)
    {
        if (!CanBePickedUp)
        {
            return Result<ParcelError>.Failure(ParcelError.NotStoredForPickup);
        }

        PickedUpAt = now;
        Status = ParcelStatus.PickedUp;

        return Result<ParcelError>.Success();
    }
}
