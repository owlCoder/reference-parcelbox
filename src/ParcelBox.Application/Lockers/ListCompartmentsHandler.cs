using ParcelBox.Application.Abstractions.Persistence;

namespace ParcelBox.Application.Lockers;

public sealed class ListCompartmentsHandler
{
    private readonly ICompartmentRepository _compartments;

    public ListCompartmentsHandler(ICompartmentRepository compartments)
    {
        _compartments = compartments;
    }

    public async Task<IReadOnlyList<CompartmentDetails>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var items = await _compartments.ListAsync(cancellationToken);

        return items
            .Select(x => new CompartmentDetails(
                x.Id,
                x.LockerCode,
                x.Number,
                x.Size,
                x.Status,
                x.ParcelId))
            .ToList();
    }
}
