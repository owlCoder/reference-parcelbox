using ParcelBox.Domain.Common;

namespace ParcelBox.Domain.Pickup;

public sealed class PickupAccess
{
    private const int MaxFailedAttempts = 3;

    private PickupAccess() { }

    private PickupAccess(Guid id, Guid parcelId, string codeHash, DateTimeOffset expiresAt)
    {
        Id = id;
        ParcelId = parcelId;
        CodeHash = codeHash;
        ExpiresAt = expiresAt;
        Status = PickupStatus.Active;
        MessageDelivery = MessageDeliveryStatus.Pending;
    }

    public Guid Id { get; private set; }
    public Guid ParcelId { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public int FailedAttempts { get; private set; }
    public PickupStatus Status { get; private set; }
    public MessageDeliveryStatus MessageDelivery { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }

    public static PickupAccess Create(Guid parcelId, string codeHash, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(codeHash))
            throw new DomainException("Pickup code hash is required.");

        return new PickupAccess(Guid.NewGuid(), parcelId, codeHash, expiresAt);
    }

    public PickupCodeValidation ValidateAttempt(string submittedHash, DateTimeOffset now)
    {
        if (Status == PickupStatus.Used)
            return PickupCodeValidation.Used;

        if (Status == PickupStatus.Locked)
            return PickupCodeValidation.Locked;

        if (now >= ExpiresAt)
            return PickupCodeValidation.Expired;

        if (CodeHash == submittedHash)
            return PickupCodeValidation.Valid;

        FailedAttempts++;
        if (FailedAttempts >= MaxFailedAttempts)
        {
            Status = PickupStatus.Locked;
            return PickupCodeValidation.Locked;
        }

        return PickupCodeValidation.Invalid;
    }

    public void MarkUsed(DateTimeOffset now)
    {
        if (Status != PickupStatus.Active)
            throw new DomainException("Pickup access is not active.");

        if (now >= ExpiresAt)
            throw new DomainException("Pickup access has expired.");

        Status = PickupStatus.Used;
        UsedAt = now;
    }

    public void MarkMessageDelivered() => MessageDelivery = MessageDeliveryStatus.Delivered;

    public void MarkMessageFailed() => MessageDelivery = MessageDeliveryStatus.Failed;
}
