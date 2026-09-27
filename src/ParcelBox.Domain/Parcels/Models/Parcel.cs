using ParcelBox.Domain.Common;

namespace ParcelBox.Domain.Parcels;

public sealed class Parcel
{
    private Parcel() { }

    private Parcel(Guid id, string trackingCode, string recipientPhone, ParcelSize size, DateTimeOffset createdAt)
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

    public static Parcel Register(string trackingCode, string recipientPhone, ParcelSize size, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
            throw new DomainException("Tracking code is required.");

        if (string.IsNullOrWhiteSpace(recipientPhone))
            throw new DomainException("Recipient phone is required.");

        return new Parcel(Guid.NewGuid(), trackingCode.Trim(), recipientPhone.Trim(), size, now);
    }

    public void Store(string lockerCode, string compartmentNumber, DateTimeOffset now)
    {
        if (Status != ParcelStatus.Registered)
            throw new DomainException("Only a registered parcel can be stored.");

        LockerCode = lockerCode;
        CompartmentNumber = compartmentNumber;
        StoredAt = now;
        Status = ParcelStatus.Stored;
    }

    public void MarkPickedUp(DateTimeOffset now)
    {
        if (Status != ParcelStatus.Stored)
            throw new DomainException("Only a stored parcel can be picked up.");

        PickedUpAt = now;
        Status = ParcelStatus.PickedUp;
    }

    public void Cancel()
    {
        if (Status != ParcelStatus.Registered)
            throw new DomainException("Only a registered parcel can be cancelled.");

        Status = ParcelStatus.Cancelled;
    }
}
