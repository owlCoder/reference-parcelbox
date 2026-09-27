using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelBox.Domain.Parcels.Models;

namespace ParcelBox.Infrastructure.Persistence.Configurations;

internal sealed class ParcelConfiguration : IEntityTypeConfiguration<Parcel>
{
    public void Configure(EntityTypeBuilder<Parcel> entity)
    {
        entity.ToTable("parcels");
        entity.HasKey(parcel => parcel.Id);
        entity.HasIndex(parcel => parcel.TrackingCode).IsUnique();
        entity.Property(parcel => parcel.TrackingCode).HasMaxLength(64);
        entity.Property(parcel => parcel.RecipientPhone).HasMaxLength(32);
        entity.Property(parcel => parcel.LockerCode).HasMaxLength(32);
        entity.Property(parcel => parcel.CompartmentNumber).HasMaxLength(16);
    }
}
