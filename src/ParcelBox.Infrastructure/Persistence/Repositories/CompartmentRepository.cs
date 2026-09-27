using Microsoft.EntityFrameworkCore;
using ParcelBox.Application.Abstractions;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Infrastructure.Persistence;

internal sealed class CompartmentRepository(ParcelBoxDbContext db) : ICompartmentRepository
{
    public Task<Compartment?> FindAvailableAsync(CompartmentSize requiredSize, CancellationToken cancellationToken) =>
        db.Compartments
            .Where(x => x.Status == CompartmentStatus.Available && (int)x.Size >= (int)requiredSize)
            .OrderBy(x => x.Size)
            .ThenBy(x => x.Number)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Compartment?> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken) =>
        db.Compartments.SingleOrDefaultAsync(x => x.ParcelId == parcelId, cancellationToken);

    public async Task<IReadOnlyList<Compartment>> ListAsync(CancellationToken cancellationToken) =>
        await db.Compartments
            .OrderBy(x => x.LockerCode)
            .ThenBy(x => x.Number)
            .ToListAsync(cancellationToken);
}
