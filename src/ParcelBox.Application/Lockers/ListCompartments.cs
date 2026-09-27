using ParcelBox.Application.Abstractions;
using ParcelBox.Domain.Lockers;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Lockers;

public sealed record CompartmentDetails(
    Guid Id,
    string LockerCode,
    string Number,
    ParcelSize Size,
    CompartmentStatus Status,
    Guid? ParcelId);

public sealed class ListCompartmentsHandler(ICompartmentRepository compartments)
{
    public async Task<IReadOnlyList<CompartmentDetails>> HandleAsync(CancellationToken cancellationToken)
    {
        var items = await compartments.ListAsync(cancellationToken);
        return items
            .Select(x => new CompartmentDetails(x.Id, x.LockerCode, x.Number, x.Size, x.Status, x.ParcelId))
            .ToList();
    }
}
