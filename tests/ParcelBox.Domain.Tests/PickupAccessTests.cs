using ParcelBox.Domain.Pickup;

namespace ParcelBox.Domain.Tests;

public sealed class PickupAccessTests
{
    [Fact]
    public void Third_invalid_attempt_locks_access()
    {
        var access = PickupAccess.Create(Guid.NewGuid(), "correct", DateTimeOffset.UtcNow.AddHours(1));

        Assert.Equal(PickupCodeValidation.Invalid, access.ValidateAttempt("wrong-1", DateTimeOffset.UtcNow));
        Assert.Equal(PickupCodeValidation.Invalid, access.ValidateAttempt("wrong-2", DateTimeOffset.UtcNow));
        Assert.Equal(PickupCodeValidation.Locked, access.ValidateAttempt("wrong-3", DateTimeOffset.UtcNow));
        Assert.Equal(PickupStatus.Locked, access.Status);
    }

    [Fact]
    public void Valid_code_does_not_mark_access_used_before_locker_opens()
    {
        var access = PickupAccess.Create(Guid.NewGuid(), "correct", DateTimeOffset.UtcNow.AddHours(1));

        var result = access.ValidateAttempt("correct", DateTimeOffset.UtcNow);

        Assert.Equal(PickupCodeValidation.Valid, result);
        Assert.Equal(PickupStatus.Active, access.Status);
    }
}
