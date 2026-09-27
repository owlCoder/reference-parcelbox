using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class CompartmentRepositoryFake : ICompartmentRepository
{
    public List<Compartment> Items { get; } = [];

    public Task<IReadOnlyList<Compartment>> GetAvailableAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Compartment> result = Items
            .Where(compartment => compartment.Status == CompartmentStatus.Available)
            .OrderBy(compartment => compartment.LockerCode)
            .ThenBy(compartment => compartment.Number)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<Compartment?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Items.SingleOrDefault(compartment => compartment.ParcelId == parcelId));
    }

    public Task<IReadOnlyList<Compartment>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Compartment> result = Items.ToList();
        return Task.FromResult(result);
    }
}
