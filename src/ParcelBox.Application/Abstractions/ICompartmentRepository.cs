using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Abstractions;

public interface ICompartmentRepository
{
    Task<Compartment?> FindAvailableAsync(ParcelSize parcelSize, CancellationToken cancellationToken);
    Task<Compartment?> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Compartment>> ListAsync(CancellationToken cancellationToken);
}
