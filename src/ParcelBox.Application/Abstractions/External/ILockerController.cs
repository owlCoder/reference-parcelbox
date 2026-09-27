using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Abstractions.External;

public interface ILockerController
{
    Task<Result> OpenAsync(
        string lockerCode,
        string compartmentNumber,
        CancellationToken cancellationToken);
}
