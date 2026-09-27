using ParcelBox.Application.Common.Results;
using ParcelBox.Application.Enums;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Interfaces.Services;

public interface ILockerService
{
    Task<Result<Compartment, LockerOperationError>> AssignAsync(
        Guid parcelId,
        SizeCategory requiredSize,
        CancellationToken cancellationToken);

    Task<Result<Compartment, LockerOperationError>> OpenForPickupAsync(
        Guid parcelId,
        CancellationToken cancellationToken);

    Result<LockerOperationError> Release(Compartment compartment, Guid parcelId);
}
