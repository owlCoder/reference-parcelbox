using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Infrastructure.Persistence.Configurations;

internal sealed class CompartmentConfiguration : IEntityTypeConfiguration<Compartment>
{
    public void Configure(EntityTypeBuilder<Compartment> entity)
    {
        entity.ToTable("compartments");
        entity.HasKey(compartment => compartment.Id);
        entity.HasIndex(compartment => new { compartment.LockerCode, compartment.Number }).IsUnique();
        entity.Property(compartment => compartment.LockerCode).HasMaxLength(32);
        entity.Property(compartment => compartment.Number).HasMaxLength(16);
    }
}
