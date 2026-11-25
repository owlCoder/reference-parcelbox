using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Services.CompartmentSelection;

public sealed class PreserveLargeCompartmentSelectionStrategy : ICompartmentSelectionStrategy
{
    public CompartmentSelectionStrategy Strategy => CompartmentSelectionStrategy.PreserveLarge;

    public IReadOnlyList<Compartment> OrderCandidates(SizeCategory requiredSize, IReadOnlyList<Compartment> candidates, IReadOnlyList<Compartment> allCompartments)
    {
        if (requiredSize == SizeCategory.Large)
        {
            return candidates
                .OrderBy(compartment => compartment.LockerCode)
                .ThenBy(compartment => compartment.Number)
                .ToList();
        }

        var availableLargeByLocker = allCompartments
            .Where(compartment =>
                compartment.Status == CompartmentStatus.Available &&
                compartment.Size == SizeCategory.Large)
            .GroupBy(compartment => compartment.LockerCode)
            .ToDictionary(locker => locker.Key, locker => locker.Count());

        return candidates
            .OrderBy(compartment =>
                availableLargeByLocker.GetValueOrDefault(compartment.LockerCode))
            .ThenBy(compartment => compartment.LockerCode)
            .ThenBy(compartment => compartment.Size)
            .ThenBy(compartment => compartment.Number)
            .ToList();
    }

    public string Explain(SizeCategory requiredSize, Compartment selected, IReadOnlyList<Compartment> allCompartments)
    {
        if (requiredSize == SizeCategory.Large)
        {
            return $"The parcel requires a Large compartment; selected {selected.LockerCode}/{selected.Number}.";
        }

        var availableLargeCount = allCompartments.Count(compartment => compartment.LockerCode == selected.LockerCode && compartment.Status == CompartmentStatus.Available &&
            compartment.Size == SizeCategory.Large);

        return $"Selected {selected.LockerCode}, which has {availableLargeCount} available Large compartments—the fewest among lockers with a compatible option—then chose its smallest compatible compartment ({selected.Size}).";
    }
}
