using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Infrastructure.Persistence.Configurations;

internal sealed class ParcelConfiguration : IEntityTypeConfiguration<Parcel>
{
    public void Configure(EntityTypeBuilder<Parcel> entity)
    {
        entity.ToTable("parcels");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.TrackingCode).IsUnique();
        entity.Property(x => x.TrackingCode).HasMaxLength(64);
        entity.Property(x => x.RecipientPhone).HasMaxLength(32);
        entity.Property(x => x.LockerCode).HasMaxLength(32);
        entity.Property(x => x.CompartmentNumber).HasMaxLength(16);
    }
}
