using ParcelBox.Application.Abstractions.External;
using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class LockerControllerFake : ILockerController
{
    private readonly Result _result;

    public LockerControllerFake(Result result)
    {
        _result = result;
    }

    public Task<Result> OpenAsync(
        string lockerCode,
        string compartmentNumber,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_result);
    }
}
