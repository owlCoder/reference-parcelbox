using ParcelBox.Application.DTOs.Lockers;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Services;

namespace ParcelBox.Application.Services;

public sealed class CompartmentService : ICompartmentService
{
    private readonly ICompartmentRepository _compartments;

    public CompartmentService(ICompartmentRepository compartments)
    {
        _compartments = compartments;
    }

    public async Task<IReadOnlyList<CompartmentDetails>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var compartments = await _compartments.GetAllAsync(cancellationToken);

        return compartments
            .Select(compartment => new CompartmentDetails(
                compartment.Id,
                compartment.LockerCode,
                compartment.Number,
                compartment.Size,
                compartment.Status,
                compartment.ParcelId))
            .ToList();
    }
}
