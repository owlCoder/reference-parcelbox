using ParcelBox.Application.Common.Results;
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

    public LockerService(
        ICompartmentRepository compartments,
        ILockerController lockerController)
    {
        _compartments = compartments;
        _lockerController = lockerController;
    }

    public async Task<Result<Compartment, LockerOperationError>> AssignAsync(
        Guid parcelId,
        SizeCategory requiredSize,
        CancellationToken cancellationToken)
    {
        if (parcelId == Guid.Empty)
        {
            return Result<Compartment, LockerOperationError>.Failure(
                LockerOperationError.InvalidParcel);
        }

        if (!Enum.IsDefined(requiredSize))
        {
            return Result<Compartment, LockerOperationError>.Failure(
                LockerOperationError.InvalidSize);
        }

        var available = await _compartments.GetAvailableAsync(cancellationToken);
        var compatibleSizes = requiredSize switch
        {
            SizeCategory.Small => new[] { SizeCategory.Small, SizeCategory.Medium, SizeCategory.Large },
            SizeCategory.Medium => new[] { SizeCategory.Medium, SizeCategory.Large },
            SizeCategory.Large => new[] { SizeCategory.Large },
            _ => Array.Empty<SizeCategory>()
        };

        Compartment? compartment = null;

        foreach (var size in compatibleSizes)
        {
            compartment = available.FirstOrDefault(candidate => candidate.Size == size);

            if (compartment is not null)
            {
                break;
            }
        }

        if (compartment is null)
        {
            return Result<Compartment, LockerOperationError>.Failure(
                LockerOperationError.NoCompatibleCompartment);
        }

        if (compartment.Status != CompartmentStatus.Available)
        {
            return Result<Compartment, LockerOperationError>.Failure(
                LockerOperationError.CompartmentUnavailable);
        }

        var openResult = await _lockerController.OpenAsync(
            compartment.LockerCode,
            compartment.Number,
            cancellationToken);

        if (openResult.IsFailure)
        {
            return Result<Compartment, LockerOperationError>.Failure(
                MapControllerError(openResult.Error));
        }

        compartment.Status = CompartmentStatus.Occupied;
        compartment.ParcelId = parcelId;

        return Result<Compartment, LockerOperationError>.Success(compartment);
    }

    public async Task<Result<Compartment, LockerOperationError>> OpenForPickupAsync(
        Guid parcelId,
        CancellationToken cancellationToken)
    {
        var compartment = await _compartments.GetByParcelIdAsync(parcelId, cancellationToken);

        if (compartment is null)
        {
            return Result<Compartment, LockerOperationError>.Failure(
                LockerOperationError.CompartmentNotFound);
        }

        if (compartment.Status != CompartmentStatus.Occupied || compartment.ParcelId != parcelId)
        {
            return Result<Compartment, LockerOperationError>.Failure(
                LockerOperationError.CompartmentStateConflict);
        }

        var openResult = await _lockerController.OpenAsync(
            compartment.LockerCode,
            compartment.Number,
            cancellationToken);

        return openResult.IsFailure
            ? Result<Compartment, LockerOperationError>.Failure(MapControllerError(openResult.Error))
            : Result<Compartment, LockerOperationError>.Success(compartment);
    }

    public Result<LockerOperationError> Release(Compartment compartment, Guid parcelId)
    {
        if (compartment.Status != CompartmentStatus.Occupied || compartment.ParcelId != parcelId)
        {
            return Result<LockerOperationError>.Failure(
                LockerOperationError.CompartmentStateConflict);
        }

        compartment.Status = CompartmentStatus.Available;
        compartment.ParcelId = null;

        return Result<LockerOperationError>.Success();
    }

    private static LockerOperationError MapControllerError(LockerControllerError error)
    {
        return error == LockerControllerError.Jammed
            ? LockerOperationError.Jammed
            : LockerOperationError.Unavailable;
    }
}
