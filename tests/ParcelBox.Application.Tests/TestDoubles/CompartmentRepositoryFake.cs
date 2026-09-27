using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class CompartmentRepositoryFake : ICompartmentRepository
{
    private readonly Compartment _compartment;

    public CompartmentRepositoryFake(Compartment compartment)
    {
        _compartment = compartment;
    }

    public Task<Compartment?> FindAvailableAsync(
        SizeCategory requiredSize,
        CancellationToken cancellationToken)
    {
        var result = _compartment.CanFit(requiredSize) ? _compartment : null;
        return Task.FromResult(result);
    }

    public Task<Compartment?> GetByParcelIdAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        var result = _compartment.ParcelId == parcelId ? _compartment : null;
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Compartment>> ListAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Compartment> result = [_compartment];
        return Task.FromResult(result);
    }
}
