using Microsoft.EntityFrameworkCore;
using ParcelBox.Domain.Lockers;

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
            Compartment.Create("PB-01", "A1", CompartmentSize.Small),
            Compartment.Create("PB-01", "A2", CompartmentSize.Small),
            Compartment.Create("PB-01", "B1", CompartmentSize.Medium),
            Compartment.Create("PB-01", "B2", CompartmentSize.Medium),
            Compartment.Create("PB-01", "C1", CompartmentSize.Large)
        };

        await db.Compartments.AddRangeAsync(compartments, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
