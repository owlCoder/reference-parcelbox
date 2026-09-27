using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Application.Abstractions.Persistence;

public interface ICompartmentRepository
{
    Task<Compartment?> FindAvailableAsync(
        SizeCategory requiredSize,
        CancellationToken cancellationToken);

    Task<Compartment?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Compartment>> ListAsync(CancellationToken cancellationToken);
}
