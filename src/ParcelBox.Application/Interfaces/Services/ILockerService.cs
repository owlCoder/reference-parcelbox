using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Lockers;
using ParcelBox.Application.Enums;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Application.Interfaces.Services;

public interface ILockerService
{
    Task<IReadOnlyList<CompartmentDetails>> GetCompartmentsAsync(
        CancellationToken cancellationToken);

    Task<Result<Compartment, LockerOperationError>> AssignAsync(
        Guid parcelId,
        SizeCategory requiredSize,
        CancellationToken cancellationToken);

    Task<Result<Compartment, LockerOperationError>> OpenForPickupAsync(
        Guid parcelId,
        CancellationToken cancellationToken);

    Result<LockerOperationError> Release(Compartment compartment, Guid parcelId);
}
