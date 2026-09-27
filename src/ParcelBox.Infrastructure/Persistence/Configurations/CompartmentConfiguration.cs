using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Infrastructure.Persistence.Configurations;

internal sealed class CompartmentConfiguration : IEntityTypeConfiguration<Compartment>
{
    public void Configure(EntityTypeBuilder<Compartment> entity)
    {
        entity.ToTable("compartments");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.LockerCode, x.Number }).IsUnique();
        entity.Property(x => x.LockerCode).HasMaxLength(32);
        entity.Property(x => x.Number).HasMaxLength(16);
    }
}
