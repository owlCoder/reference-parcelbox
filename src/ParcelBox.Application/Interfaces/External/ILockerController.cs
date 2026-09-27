using ParcelBox.Application.Common.Results;

namespace ParcelBox.Application.Interfaces.External;

public interface ILockerController
{
    Task<Result<LockerControllerError>> OpenAsync(
        string lockerCode,
        string compartmentNumber,
        CancellationToken cancellationToken);
}
