using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Services.CompartmentSelection;

public sealed class CompactCompartmentSelectionStrategy : ICompartmentSelectionStrategy
{
    public CompartmentSelectionStrategy Strategy => CompartmentSelectionStrategy.Compact;

    public IReadOnlyList<Compartment> OrderCandidates(SizeCategory requiredSize, IReadOnlyList<Compartment> candidates, IReadOnlyList<Compartment> allCompartments)
    {
        return candidates
            .OrderBy(compartment => compartment.Size)
            .ThenBy(compartment => compartment.LockerCode)
            .ThenBy(compartment => compartment.Number)
            .ToList();
    }

    public string Explain(SizeCategory requiredSize, Compartment selected, IReadOnlyList<Compartment> allCompartments)
    {
        return $"Selected the smallest compatible {selected.Size} compartment to minimize unused space for a {requiredSize} parcel.";
    }
}
