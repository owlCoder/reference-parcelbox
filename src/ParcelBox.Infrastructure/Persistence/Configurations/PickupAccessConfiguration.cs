using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelBox.Domain.Pickup.Models;

namespace ParcelBox.Infrastructure.Persistence.Configurations;

internal sealed class PickupAccessConfiguration : IEntityTypeConfiguration<PickupAccess>
{
    public void Configure(EntityTypeBuilder<PickupAccess> entity)
    {
        entity.ToTable("pickup_accesses");
        entity.HasKey(access => access.Id);
        entity.HasIndex(access => access.ParcelId).IsUnique();
        entity.Property(access => access.CodeHash).HasMaxLength(128);
    }
}
