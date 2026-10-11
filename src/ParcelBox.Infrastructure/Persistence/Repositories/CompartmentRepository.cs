using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Infrastructure.Persistence.Repositories;

internal sealed class CompartmentRepository : ICompartmentRepository
{
    private readonly ParcelBoxDbContext _db;

    public CompartmentRepository(ParcelBoxDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Compartment>> GetAvailableAsync(CancellationToken cancellationToken)
    {
        return await _db.Compartments
            .Where(compartment => compartment.Status == CompartmentStatus.Available)
            .OrderBy(compartment => compartment.LockerCode)
            .ThenBy(compartment => compartment.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClaimResult> TryReserveAsync(Guid compartmentId, Guid parcelId, CancellationToken cancellationToken)
    {
        try
        {
            var changed = await _db.Compartments
                .Where(compartment => compartment.Id == compartmentId && compartment.Status == CompartmentStatus.Available)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(compartment => compartment.Status, CompartmentStatus.Reserved)
                    .SetProperty(compartment => compartment.ParcelId, parcelId), cancellationToken);

            return changed == 1 ? ClaimResult.Claimed : ClaimResult.Conflict;
        }
        catch (DbUpdateException)
        {
            return ClaimResult.Unavailable;
        }
        catch (SqliteException)
        {
            return ClaimResult.Unavailable;
        }
    }

    public async Task<Compartment> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        return await _db.Compartments.SingleOrDefaultAsync(
            compartment => compartment.ParcelId == parcelId,
            cancellationToken) ?? new Compartment();
    }

    public async Task<IReadOnlyList<Compartment>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _db.Compartments
            .AsNoTracking()
            .OrderBy(compartment => compartment.LockerCode)
            .ThenBy(compartment => compartment.Number)
            .ToListAsync(cancellationToken);
    }
}
