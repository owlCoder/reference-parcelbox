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

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ParcelBoxDbContext).Assembly);
}
