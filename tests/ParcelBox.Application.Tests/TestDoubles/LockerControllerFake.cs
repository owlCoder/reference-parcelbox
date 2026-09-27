using ParcelBox.Application.Common.Results;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.External;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class LockerControllerFake : ILockerController
{
    public LockerControllerError Error { get; set; }

    public Task<Result<LockerControllerError>> OpenAsync(
        string lockerCode,
        string compartmentNumber,
        CancellationToken cancellationToken)
    {
        var result = Error == LockerControllerError.None
            ? Result<LockerControllerError>.Success()
            : Result<LockerControllerError>.Failure(Error);

        return Task.FromResult(result);
    }
}
