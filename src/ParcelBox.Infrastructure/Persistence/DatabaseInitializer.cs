using Microsoft.EntityFrameworkCore;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;

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

        foreach (var lockerCode in new[] { "PB-01", "PB-02" })
        {
            if (await _db.Compartments.AnyAsync(
                    compartment => compartment.LockerCode == lockerCode,
                    cancellationToken))
            {
                continue;
            }

            await _db.Compartments.AddRangeAsync(
                CreateCompartments(lockerCode),
                cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static Compartment[] CreateCompartments(string lockerCode)
    {
        return
        [
            NewCompartment(lockerCode, "A1", SizeCategory.Small),
            NewCompartment(lockerCode, "A2", SizeCategory.Small),
            NewCompartment(lockerCode, "B1", SizeCategory.Medium),
            NewCompartment(lockerCode, "B2", SizeCategory.Medium),
            NewCompartment(lockerCode, "C1", SizeCategory.Large)
        ];
    }

    private static Compartment NewCompartment(
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
