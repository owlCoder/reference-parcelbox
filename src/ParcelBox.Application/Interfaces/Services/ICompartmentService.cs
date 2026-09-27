using ParcelBox.Application.DTOs.Lockers;

namespace ParcelBox.Application.Interfaces.Services;

public interface ICompartmentService
{
    Task<IReadOnlyList<CompartmentDetails>> GetAllAsync(
        CancellationToken cancellationToken);
}
