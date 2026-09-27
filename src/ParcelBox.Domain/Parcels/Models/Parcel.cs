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
        ParcelSize size,
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

    public ParcelSize Size { get; private set; }

    public ParcelStatus Status { get; private set; }

    public string? LockerCode { get; private set; }

    public string? CompartmentNumber { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? StoredAt { get; private set; }

    public DateTimeOffset? PickedUpAt { get; private set; }

    public bool CanBeStored => Status == ParcelStatus.Registered;

    public bool CanBePickedUp => Status == ParcelStatus.Stored;

    public static Result<Parcel> Register(
        string trackingCode,
        string recipientPhone,
        ParcelSize size,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
        {
            return Result<Parcel>.Failure(ParcelErrors.TrackingCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(recipientPhone))
        {
            return Result<Parcel>.Failure(ParcelErrors.RecipientPhoneRequired);
        }

        if (!Enum.IsDefined(size))
        {
            return Result<Parcel>.Failure(ParcelErrors.InvalidSize);
        }

        var parcel = new Parcel(
            Guid.NewGuid(),
            trackingCode.Trim(),
            recipientPhone.Trim(),
            size,
            now);

        return Result<Parcel>.Success(parcel);
    }

    public Result Store(string lockerCode, string compartmentNumber, DateTimeOffset now)
    {
        if (!CanBeStored)
        {
            return Result.Failure(ParcelErrors.NotRegisteredForStorage);
        }

        if (string.IsNullOrWhiteSpace(lockerCode) || string.IsNullOrWhiteSpace(compartmentNumber))
        {
            return Result.Failure(ParcelErrors.StorageLocationRequired);
        }

        LockerCode = lockerCode.Trim();
        CompartmentNumber = compartmentNumber.Trim();
        StoredAt = now;
        Status = ParcelStatus.Stored;

        return Result.Success();
    }

    public Result MarkPickedUp(DateTimeOffset now)
    {
        if (!CanBePickedUp)
        {
            return Result.Failure(ParcelErrors.NotStoredForPickup);
        }

        PickedUpAt = now;
        Status = ParcelStatus.PickedUp;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != ParcelStatus.Registered)
        {
            return Result.Failure(ParcelErrors.NotRegisteredForCancellation);
        }

        Status = ParcelStatus.Cancelled;
        return Result.Success();
    }
}
