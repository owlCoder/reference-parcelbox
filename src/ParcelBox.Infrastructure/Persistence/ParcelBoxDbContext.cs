using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Abstractions;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Infrastructure.Persistence;

public sealed class ParcelBoxDbContext(DbContextOptions<ParcelBoxDbContext> options)
    : DbContext(options), IAppDbSession
{
    public DbSet<Parcel> Parcels => Set<Parcel>();
    public DbSet<Compartment> Compartments => Set<Compartment>();
    public DbSet<PickupAccess> PickupAccesses => Set<PickupAccess>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Parcel>(entity =>
        {
            entity.ToTable("parcels");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TrackingCode).IsUnique();
            entity.Property(x => x.TrackingCode).HasMaxLength(64);
            entity.Property(x => x.RecipientPhone).HasMaxLength(32);
            entity.Property(x => x.LockerCode).HasMaxLength(32);
            entity.Property(x => x.CompartmentNumber).HasMaxLength(16);
        });

        modelBuilder.Entity<Compartment>(entity =>
        {
            entity.ToTable("compartments");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.LockerCode, x.Number }).IsUnique();
            entity.Property(x => x.LockerCode).HasMaxLength(32);
            entity.Property(x => x.Number).HasMaxLength(16);
        });

        modelBuilder.Entity<PickupAccess>(entity =>
        {
            entity.ToTable("pickup_accesses");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ParcelId).IsUnique();
            entity.Property(x => x.CodeHash).HasMaxLength(128);
        });
    }
}
