using System.Globalization;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Services.CompartmentSelection;

public sealed class BalancedCompartmentSelectionStrategy : ICompartmentSelectionStrategy
{
    public CompartmentSelectionStrategy Strategy => CompartmentSelectionStrategy.Balanced;

    public IReadOnlyList<Compartment> OrderCandidates(SizeCategory requiredSize, IReadOnlyList<Compartment> candidates, IReadOnlyList<Compartment> allCompartments)
    {
        var utilizationByLocker = allCompartments
            .GroupBy(compartment => compartment.LockerCode)
            .ToDictionary(
                locker => locker.Key,
                locker => (double)locker.Count(
                    compartment => compartment.Status != CompartmentStatus.Available) / locker.Count());

        return candidates
            .OrderBy(compartment => utilizationByLocker[compartment.LockerCode])
            .ThenBy(compartment => compartment.LockerCode)
            .ThenBy(compartment => compartment.Size)
            .ThenBy(compartment => compartment.Number)
            .ToList();
    }

    public string Explain(SizeCategory requiredSize, Compartment selected, IReadOnlyList<Compartment> allCompartments)
    {
        var lockerCompartments = allCompartments
            .Where(compartment => compartment.LockerCode == selected.LockerCode)
            .ToArray();
        var occupiedCompartments = lockerCompartments.Count(compartment => compartment.Status != CompartmentStatus.Available);
        var utilization = (double)occupiedCompartments / lockerCompartments.Length;
        var percentage = utilization.ToString("P0", CultureInfo.InvariantCulture);

        return $"Selected {selected.LockerCode}, the least-utilized compatible locker ({percentage} full), then chose its smallest compatible compartment ({selected.Size}) for the {requiredSize} parcel.";
    }
}
