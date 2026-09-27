using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Infrastructure.Persistence;

internal sealed class CompartmentRepository : ICompartmentRepository
{
    private readonly ParcelBoxDbContext _db;

    public CompartmentRepository(ParcelBoxDbContext db)
    {
        _db = db;
    }

    public Task<Compartment?> FindAvailableAsync(
        SizeCategory requiredSize,
        CancellationToken cancellationToken)
    {
        return _db.Compartments
            .Where(x => x.Status == CompartmentStatus.Available && x.Size >= requiredSize)
            .OrderBy(x => x.Size)
            .ThenBy(x => x.Number)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Compartment?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        return _db.Compartments.SingleOrDefaultAsync(
            x => x.ParcelId == parcelId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Compartment>> ListAsync(CancellationToken cancellationToken)
    {
        return await _db.Compartments
            .OrderBy(x => x.LockerCode)
            .ThenBy(x => x.Number)
            .ToListAsync(cancellationToken);
    }
}
