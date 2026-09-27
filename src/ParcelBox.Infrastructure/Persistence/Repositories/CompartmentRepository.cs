using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Infrastructure.Persistence;

internal sealed class CompartmentRepository : ICompartmentRepository
{
    private readonly ParcelBoxDbContext _db;

    public CompartmentRepository(ParcelBoxDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Compartment>> GetAvailableAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Compartments
            .Where(x => x.Status == CompartmentStatus.Available)
            .OrderBy(x => x.LockerCode)
            .ThenBy(x => x.Number)
            .ToListAsync(cancellationToken);
    }

    public Task<Compartment?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        return _db.Compartments.SingleOrDefaultAsync(
            x => x.ParcelId == parcelId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Compartment>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Compartments
            .OrderBy(x => x.LockerCode)
            .ThenBy(x => x.Number)
            .ToListAsync(cancellationToken);
    }
}
