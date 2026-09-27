using ParcelBox.Domain.Lockers;

namespace ParcelBox.Application.Abstractions.Persistence;

public interface ICompartmentRepository
{
    Task<Compartment?> FindAvailableAsync(
        CompartmentSize requiredSize,
        CancellationToken cancellationToken);

    Task<Compartment?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Compartment>> ListAsync(CancellationToken cancellationToken);
}
