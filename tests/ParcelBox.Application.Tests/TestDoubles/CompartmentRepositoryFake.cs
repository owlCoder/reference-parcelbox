using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class CompartmentRepositoryFake : ICompartmentRepository
{
    public List<Compartment> Items { get; } = [];

    public Task<IReadOnlyList<Compartment>> GetAvailableAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Compartment> result = Items
            .Where(compartment => compartment.Status == CompartmentStatus.Available)
            .OrderBy(compartment => compartment.LockerCode)
            .ThenBy(compartment => compartment.Number)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<ClaimResult> TryReserveAsync(Guid compartmentId, Guid parcelId, CancellationToken cancellationToken)
    {
        lock (Items)
        {
            var compartment = Items.FirstOrDefault(item => item.Id == compartmentId);

            if (compartment is null || compartment.Status != CompartmentStatus.Available)
            {
                return Task.FromResult(ClaimResult.Conflict);
            }

            compartment.Status = CompartmentStatus.Reserved;
            compartment.ParcelId = parcelId;
            return Task.FromResult(ClaimResult.Claimed);
        }
    }

    public Task<Compartment> GetByParcelIdAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Items.SingleOrDefault(compartment => compartment.ParcelId == parcelId) ?? new Compartment());
    }

    public Task<IReadOnlyList<Compartment>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Compartment> result = Items.ToList();
        return Task.FromResult(result);
    }
}
