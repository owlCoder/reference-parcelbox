using Microsoft.EntityFrameworkCore;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Common.Results;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly ParcelBoxDbContext _db;

    public DatabaseInitializer(ParcelBoxDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CompartmentError>> InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await _db.Database.EnsureCreatedAsync(cancellationToken);

        if (await _db.Compartments.AnyAsync(cancellationToken))
        {
            return Result<CompartmentError>.Success();
        }

        var definitions = new (string LockerCode, string Number, SizeCategory Size)[]
        {
            ("PB-01", "A1", SizeCategory.Small),
            ("PB-01", "A2", SizeCategory.Small),
            ("PB-01", "B1", SizeCategory.Medium),
            ("PB-01", "B2", SizeCategory.Medium),
            ("PB-01", "C1", SizeCategory.Large)
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
                return Result<CompartmentError>.Failure(createResult.Error);
            }

            compartments.Add(createResult.Value);
        }

        await _db.Compartments.AddRangeAsync(compartments, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CompartmentError>.Success();
    }
}
