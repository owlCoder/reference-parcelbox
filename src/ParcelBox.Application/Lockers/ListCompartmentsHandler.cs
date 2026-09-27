using ParcelBox.Application.Abstractions;

namespace ParcelBox.Application.Lockers;

public sealed class ListCompartmentsHandler(ICompartmentRepository compartments)
{
    public async Task<IReadOnlyList<CompartmentDetails>> HandleAsync(CancellationToken cancellationToken)
    {
        var items = await compartments.ListAsync(cancellationToken);

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
