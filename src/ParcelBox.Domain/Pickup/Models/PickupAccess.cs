using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Domain.Pickup;

public sealed class PickupAccess
{
    private const int MaxFailedAttempts = 3;

    private PickupAccess()
    {
    }

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

    public static Result<PickupAccess> Create(Guid parcelId, string codeHash, DateTimeOffset expiresAt)
    {
        if (parcelId == Guid.Empty)
        {
            return Result<PickupAccess>.Failure(PickupAccessErrors.ParcelIdRequired);
        }

        if (string.IsNullOrWhiteSpace(codeHash))
        {
            return Result<PickupAccess>.Failure(PickupAccessErrors.CodeHashRequired);
        }

        if (expiresAt == default)
        {
            return Result<PickupAccess>.Failure(PickupAccessErrors.ExpirationRequired);
        }

        var access = new PickupAccess(Guid.NewGuid(), parcelId, codeHash, expiresAt);
        return Result<PickupAccess>.Success(access);
    }

    public PickupCodeValidation ValidateAttempt(string submittedHash, DateTimeOffset now)
    {
        if (Status == PickupStatus.Used)
        {
            return PickupCodeValidation.Used;
        }

        if (Status == PickupStatus.Locked)
        {
            return PickupCodeValidation.Locked;
        }

        if (now >= ExpiresAt)
        {
            return PickupCodeValidation.Expired;
        }

        if (CodeHash == submittedHash)
        {
            return PickupCodeValidation.Valid;
        }

        FailedAttempts++;

        if (FailedAttempts >= MaxFailedAttempts)
        {
            Status = PickupStatus.Locked;
            return PickupCodeValidation.Locked;
        }

        return PickupCodeValidation.Invalid;
    }

    public Result MarkUsed(DateTimeOffset now)
    {
        if (Status != PickupStatus.Active)
        {
            return Result.Failure(PickupAccessErrors.NotActive);
        }

        if (now >= ExpiresAt)
        {
            return Result.Failure(PickupAccessErrors.Expired);
        }

        Status = PickupStatus.Used;
        UsedAt = now;

        return Result.Success();
    }

    public void MarkMessageDelivered()
    {
        MessageDelivery = MessageDeliveryStatus.Delivered;
    }

    public void MarkMessageFailed()
    {
        MessageDelivery = MessageDeliveryStatus.Failed;
    }
}
