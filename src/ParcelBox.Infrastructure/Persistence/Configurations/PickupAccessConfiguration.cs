using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Infrastructure.Persistence.Configurations;

internal sealed class PickupAccessConfiguration : IEntityTypeConfiguration<PickupAccess>
{
    public void Configure(EntityTypeBuilder<PickupAccess> entity)
    {
        entity.ToTable("pickup_accesses");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.ParcelId).IsUnique();
        entity.Property(x => x.CodeHash).HasMaxLength(128);
    }
}
