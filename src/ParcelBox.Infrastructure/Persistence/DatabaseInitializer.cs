using Microsoft.EntityFrameworkCore;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(ParcelBoxDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.Compartments.AnyAsync(cancellationToken))
            return;

        var compartments = new[]
        {
            new Compartment(Guid.NewGuid(), "PB-01", "A1", ParcelSize.Small),
            new Compartment(Guid.NewGuid(), "PB-01", "A2", ParcelSize.Small),
            new Compartment(Guid.NewGuid(), "PB-01", "B1", ParcelSize.Medium),
            new Compartment(Guid.NewGuid(), "PB-01", "B2", ParcelSize.Medium),
            new Compartment(Guid.NewGuid(), "PB-01", "C1", ParcelSize.Large)
        };

        await db.Compartments.AddRangeAsync(compartments, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
