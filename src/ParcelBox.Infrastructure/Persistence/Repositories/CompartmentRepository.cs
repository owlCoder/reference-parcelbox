using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyList<Compartment>> GetAvailableAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Compartments
            .Where(compartment => compartment.Status == CompartmentStatus.Available)
            .OrderBy(compartment => compartment.LockerCode)
            .ThenBy(compartment => compartment.Number)
            .ToListAsync(cancellationToken);
    }

    public Task<Compartment?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        return _db.Compartments.SingleOrDefaultAsync(
            compartment => compartment.ParcelId == parcelId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Compartment>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Compartments
            .AsNoTracking()
            .OrderBy(compartment => compartment.LockerCode)
            .ThenBy(compartment => compartment.Number)
            .ToListAsync(cancellationToken);
    }
}
