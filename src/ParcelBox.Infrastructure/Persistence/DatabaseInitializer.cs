using Microsoft.EntityFrameworkCore;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly ParcelBoxDbContext _db;

    public DatabaseInitializer(ParcelBoxDbContext db)
    {
        _db = db;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.EnsureCreatedAsync(cancellationToken);

        if (await _db.Compartments.AnyAsync(cancellationToken))
        {
            return;
        }

        var compartments = new[]
        {
            CreateCompartment("PB-01", "A1", SizeCategory.Small),
            CreateCompartment("PB-01", "A2", SizeCategory.Small),
            CreateCompartment("PB-01", "B1", SizeCategory.Medium),
            CreateCompartment("PB-01", "B2", SizeCategory.Medium),
            CreateCompartment("PB-01", "C1", SizeCategory.Large)
        };

        await _db.Compartments.AddRangeAsync(compartments, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static Compartment CreateCompartment(
        string lockerCode,
        string number,
        SizeCategory size)
    {
        return new Compartment
        {
            Id = Guid.NewGuid(),
            LockerCode = lockerCode,
            Number = number,
            Size = size,
            Status = CompartmentStatus.Available
        };
    }
}
