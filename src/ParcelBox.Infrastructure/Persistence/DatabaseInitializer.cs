using Microsoft.EntityFrameworkCore;
using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task<Result> InitializeAsync(
        ParcelBoxDbContext db,
        CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.Compartments.AnyAsync(cancellationToken))
        {
            return Result.Success();
        }

        var definitions = new (string LockerCode, string Number, CompartmentSize Size)[]
        {
            ("PB-01", "A1", CompartmentSize.Small),
            ("PB-01", "A2", CompartmentSize.Small),
            ("PB-01", "B1", CompartmentSize.Medium),
            ("PB-01", "B2", CompartmentSize.Medium),
            ("PB-01", "C1", CompartmentSize.Large)
        };

        var compartments = new List<Compartment>();

        foreach (var definition in definitions)
        {
            var createResult = Compartment.Create(
                definition.LockerCode,
                definition.Number,
                definition.Size);

            if (createResult.IsFailure)
            {
                return Result.Failure(createResult.Error);
            }

            compartments.Add(createResult.Value);
        }

        await db.Compartments.AddRangeAsync(compartments, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
