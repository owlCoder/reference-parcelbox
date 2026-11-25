using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Lockers;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.External;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Services;

public sealed class LockerService : ILockerService
{
    private readonly ICompartmentRepository _compartments;
    private readonly ILockerController _lockerController;
    private readonly IReadOnlyDictionary<CompartmentSelectionStrategy, ICompartmentSelectionStrategy> _selectionStrategies;

    public LockerService(ICompartmentRepository compartments, ILockerController lockerController, IEnumerable<ICompartmentSelectionStrategy> selectionStrategies)
    {
        _compartments = compartments;
        _lockerController = lockerController;
        _selectionStrategies = selectionStrategies.ToDictionary(strategy => strategy.Strategy);
    }

    public async Task<Result<CompartmentAssignmentDto, LockerOperationError>> AssignAsync(Guid parcelId, SizeCategory requiredSize, CompartmentSelectionStrategy strategy, CancellationToken cancellationToken)
    {
        if (parcelId == Guid.Empty)
        {
            return Result<CompartmentAssignmentDto, LockerOperationError>.Failure(LockerOperationError.InvalidParcel);
        }

        if (!Enum.IsDefined(requiredSize))
        {
            return Result<CompartmentAssignmentDto, LockerOperationError>.Failure(LockerOperationError.InvalidSize);
        }

        if (!_selectionStrategies.TryGetValue(strategy, out var selectionStrategy))
        {
            return Result<CompartmentAssignmentDto, LockerOperationError>.Failure(LockerOperationError.InvalidStrategy);
        }

        var allCompartments = await _compartments.GetAllForAssignmentAsync(cancellationToken);
        var candidates = allCompartments
            .Where(compartment =>
                compartment.Status == CompartmentStatus.Available &&
                IsCompatible(requiredSize, compartment.Size))
            .ToArray();

        if (candidates.Length == 0)
        {
            return Result<CompartmentAssignmentDto, LockerOperationError>.Failure(LockerOperationError.NoCompatibleCompartment);
        }

        var orderedCandidates = selectionStrategy.OrderCandidates(requiredSize, candidates, allCompartments);

        Compartment? selected = null;
        var selectionReason = string.Empty;

        foreach (var candidate in orderedCandidates)
        {
            var claim = await _compartments.TryReserveAsync(candidate.Id, parcelId, cancellationToken);

            if (claim == ClaimResult.Unavailable)
            {
                return Result<CompartmentAssignmentDto, LockerOperationError>.Failure(LockerOperationError.PersistenceUnavailable);
            }

            if (claim != ClaimResult.Claimed)
            {
                continue;
            }

            candidate.Status = CompartmentStatus.Reserved;
            candidate.ParcelId = parcelId;
            selected = candidate;
            selectionReason = selectionStrategy.Explain(requiredSize, selected, allCompartments);
            break;
        }

        if (selected is null)
        {
            return Result<CompartmentAssignmentDto, LockerOperationError>.Failure(LockerOperationError.NoCompatibleCompartment);
        }

        if (selected.Status != CompartmentStatus.Reserved || selected.ParcelId != parcelId)
        {
            return Result<CompartmentAssignmentDto, LockerOperationError>.Failure(LockerOperationError.CompartmentStateConflict);
        }

        var openResult = await _lockerController.OpenAsync(selected.LockerCode, selected.Number, cancellationToken);

        if (openResult.IsFailure)
        {
            return Result<CompartmentAssignmentDto, LockerOperationError>.Failure(MapControllerError(openResult.Error));
        }

        selected.Status = CompartmentStatus.Occupied;
        selected.ParcelId = parcelId;

        return Result<CompartmentAssignmentDto, LockerOperationError>.Success(new CompartmentAssignmentDto(selected, strategy, selectionReason));
    }

    public async Task<Result<Compartment, LockerOperationError>> OpenForPickupAsync(Guid parcelId, CancellationToken cancellationToken)
    {
        var compartment = await _compartments.GetByParcelIdAsync(parcelId, cancellationToken);

        if (compartment.Id == Guid.Empty)
        {
            return Result<Compartment, LockerOperationError>.Failure(LockerOperationError.CompartmentNotFound);
        }

        if (compartment.Status != CompartmentStatus.Occupied || compartment.ParcelId != parcelId)
        {
            return Result<Compartment, LockerOperationError>.Failure(LockerOperationError.CompartmentStateConflict);
        }

        var openResult = await _lockerController.OpenAsync(compartment.LockerCode, compartment.Number, cancellationToken);
        return openResult.IsFailure
            ? Result<Compartment, LockerOperationError>.Failure(MapControllerError(openResult.Error))
            : Result<Compartment, LockerOperationError>.Success(compartment);
    }

    public Result<LockerOperationError> Release(Compartment compartment, Guid parcelId)
    {
        if (compartment.Status != CompartmentStatus.Occupied || compartment.ParcelId != parcelId)
        {
            return Result<LockerOperationError>.Failure(LockerOperationError.CompartmentStateConflict);
        }

        compartment.Status = CompartmentStatus.Available;
        compartment.ParcelId = null;

        return Result<LockerOperationError>.Success();
    }

    private static bool IsCompatible(SizeCategory requiredSize, SizeCategory compartmentSize)
    {
        return requiredSize switch
        {
            SizeCategory.Small => compartmentSize is SizeCategory.Small or SizeCategory.Medium or SizeCategory.Large,
            SizeCategory.Medium => compartmentSize is SizeCategory.Medium or SizeCategory.Large,
            SizeCategory.Large => compartmentSize == SizeCategory.Large,
            _ => false
        };
    }

    private static LockerOperationError MapControllerError(LockerControllerError error)
    {
        return error == LockerControllerError.Jammed
            ? LockerOperationError.Jammed
            : LockerOperationError.Unavailable;
    }
}
