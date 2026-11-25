using ParcelBox.Application.Enums;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Interfaces.Services;

public interface ICompartmentSelectionStrategy
{
    CompartmentSelectionStrategy Strategy { get; }

    IReadOnlyList<Compartment> OrderCandidates(SizeCategory requiredSize, IReadOnlyList<Compartment> candidates, IReadOnlyList<Compartment> allCompartments);

    string Explain(SizeCategory requiredSize, Compartment selected, IReadOnlyList<Compartment> allCompartments);
}
