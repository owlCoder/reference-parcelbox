using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Interfaces.Persistence;
using ParcelBox.Domain.Lockers.Models;
using ParcelBox.Domain.Parcels.Models;
using ParcelBox.Domain.Pickup.Models;

namespace ParcelBox.Infrastructure.Persistence;

public sealed class ParcelBoxDbContext : DbContext, IUnitOfWork
{
    public ParcelBoxDbContext(DbContextOptions<ParcelBoxDbContext> options)
        : base(options)
    {
    }

    public DbSet<Parcel> Parcels => Set<Parcel>();
    public DbSet<Compartment> Compartments => Set<Compartment>();
    public DbSet<PickupAccess> PickupAccesses => Set<PickupAccess>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ParcelBoxDbContext).Assembly);
    }
}
