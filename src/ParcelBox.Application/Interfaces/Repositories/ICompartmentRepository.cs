using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Interfaces.Repositories;

public interface ICompartmentRepository
{
    Task<IReadOnlyList<Compartment>> GetAvailableAsync(CancellationToken cancellationToken);

    Task<Compartment?> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Compartment>> GetAllAsync(CancellationToken cancellationToken);
}
