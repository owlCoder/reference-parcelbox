using ParcelBox.Application.Abstractions.External;
using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class MessageGatewayFake : IMessageGateway
{
    private readonly Result _result;

    public MessageGatewayFake(Result result)
    {
        _result = result;
    }

    public Task<Result> SendPickupCodeAsync(
        string destination,
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_result);
    }
}
