using ParcelBox.Domain.Pickup;

namespace ParcelBox.Domain.Tests;

public sealed class PickupAccessTests
{
    [Fact]
    public void Third_invalid_attempt_locks_access()
    {
        var now = DateTimeOffset.UtcNow;
        var createResult = PickupAccess.Create(
            Guid.NewGuid(),
            "hash",
            now.AddHours(1),
            now);

        Assert.True(createResult.IsSuccess);

        var access = createResult.Value;

        Assert.Equal(
            PickupCodeValidation.Invalid,
            access.ValidateAttempt(false, now));
        Assert.Equal(
            PickupCodeValidation.Invalid,
            access.ValidateAttempt(false, now));
        Assert.Equal(
            PickupCodeValidation.Locked,
            access.ValidateAttempt(false, now));
        Assert.Equal(PickupStatus.Locked, access.Status);
    }

    [Fact]
    public void Valid_code_does_not_mark_access_used_before_locker_opens()
    {
        var now = DateTimeOffset.UtcNow;
        var createResult = PickupAccess.Create(
            Guid.NewGuid(),
            "hash",
            now.AddHours(1),
            now);

        Assert.True(createResult.IsSuccess);

        var access = createResult.Value;
        var validation = access.ValidateAttempt(true, now);

        Assert.Equal(PickupCodeValidation.Valid, validation);
        Assert.Equal(PickupStatus.Active, access.Status);
    }

    [Fact]
    public void Expired_access_cannot_be_marked_as_used()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddMinutes(1);
        var createResult = PickupAccess.Create(
            Guid.NewGuid(),
            "hash",
            expiresAt,
            createdAt);

        Assert.True(createResult.IsSuccess);

        var result = createResult.Value.MarkUsed(expiresAt);

        Assert.True(result.IsFailure);
        Assert.Equal(PickupAccessError.Expired, result.Error);
    }

    [Fact]
    public void Access_cannot_be_created_with_expiration_in_the_past()
    {
        var now = DateTimeOffset.UtcNow;

        var result = PickupAccess.Create(
            Guid.NewGuid(),
            "hash",
            now.AddMinutes(-1),
            now);

        Assert.True(result.IsFailure);
        Assert.Equal(PickupAccessError.ExpirationMustBeInFuture, result.Error);
    }
}
