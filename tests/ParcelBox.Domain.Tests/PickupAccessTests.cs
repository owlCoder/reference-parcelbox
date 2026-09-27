using ParcelBox.Domain.Pickup;

namespace ParcelBox.Domain.Tests;

public sealed class PickupAccessTests
{
    [Fact]
    public void Third_invalid_attempt_locks_access()
    {
        var createResult = PickupAccess.Create(
            Guid.NewGuid(),
            "correct",
            DateTimeOffset.UtcNow.AddHours(1));

        Assert.True(createResult.IsSuccess);

        var access = createResult.Value;

        Assert.Equal(
            PickupCodeValidation.Invalid,
            access.ValidateAttempt("wrong-1", DateTimeOffset.UtcNow));
        Assert.Equal(
            PickupCodeValidation.Invalid,
            access.ValidateAttempt("wrong-2", DateTimeOffset.UtcNow));
        Assert.Equal(
            PickupCodeValidation.Locked,
            access.ValidateAttempt("wrong-3", DateTimeOffset.UtcNow));
        Assert.Equal(PickupStatus.Locked, access.Status);
    }

    [Fact]
    public void Valid_code_does_not_mark_access_used_before_locker_opens()
    {
        var createResult = PickupAccess.Create(
            Guid.NewGuid(),
            "correct",
            DateTimeOffset.UtcNow.AddHours(1));

        Assert.True(createResult.IsSuccess);

        var access = createResult.Value;
        var validation = access.ValidateAttempt("correct", DateTimeOffset.UtcNow);

        Assert.Equal(PickupCodeValidation.Valid, validation);
        Assert.Equal(PickupStatus.Active, access.Status);
    }

    [Fact]
    public void Expired_access_cannot_be_marked_as_used()
    {
        var now = DateTimeOffset.UtcNow;
        var createResult = PickupAccess.Create(
            Guid.NewGuid(),
            "correct",
            now.AddMinutes(-1));

        Assert.True(createResult.IsSuccess);

        var result = createResult.Value.MarkUsed(now);

        Assert.True(result.IsFailure);
        Assert.Equal(PickupAccessErrors.Expired, result.Error);
    }
}
