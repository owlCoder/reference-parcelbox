using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class CompartmentRepositoryFake : ICompartmentRepository
{
    public List<Compartment> Items { get; } = [];

    public Task<IReadOnlyList<Compartment>> GetAvailableAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Compartment> result = Items
            .Where(x => x.Status == CompartmentStatus.Available)
            .OrderBy(x => x.LockerCode)
            .ThenBy(x => x.Number)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<Compartment?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Items.SingleOrDefault(x => x.ParcelId == parcelId));
    }

    public Task<IReadOnlyList<Compartment>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Compartment> result = Items.ToList();
        return Task.FromResult(result);
    }
}
