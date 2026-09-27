using ParcelBox.Application.Abstractions.External;
using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class LockerControllerFake : ILockerController
{
    private readonly Result<LockerControllerError> _result;

    public LockerControllerFake(Result<LockerControllerError> result)
    {
        _result = result;
    }

    public Task<Result<LockerControllerError>> OpenAsync(
        string lockerCode,
        string compartmentNumber,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_result);
    }
}
