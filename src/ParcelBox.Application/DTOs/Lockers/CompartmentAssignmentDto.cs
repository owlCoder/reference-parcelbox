using ParcelBox.Application.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.DTOs.Lockers;

public sealed class CompartmentAssignmentDto
{
    public Compartment Compartment { get; set; } = new();
    public CompartmentSelectionStrategy Strategy { get; set; }
    public string SelectionReason { get; set; } = string.Empty;

    public CompartmentAssignmentDto()
    {
    }

    public CompartmentAssignmentDto(Compartment compartment, CompartmentSelectionStrategy strategy, string selectionReason)
    {
        Compartment = compartment;
        Strategy = strategy;
        SelectionReason = selectionReason;
    }
}
