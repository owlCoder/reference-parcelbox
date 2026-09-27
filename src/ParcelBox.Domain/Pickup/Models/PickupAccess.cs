using ParcelBox.Domain.Pickup.Enums;

namespace ParcelBox.Domain.Pickup.Models;

public sealed class PickupAccess
{
    public Guid Id { get; set; }
    public Guid ParcelId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public int FailedAttempts { get; set; }
    public PickupStatus Status { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
}
