using ParcelBox.Application.Abstractions.External;
using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class MessageGatewayFake : IMessageGateway
{
    private readonly Result<MessageGatewayError> _result;

    public MessageGatewayFake(Result<MessageGatewayError> result)
    {
        _result = result;
    }

    public OutboundMessage? LastMessage { get; private set; }

    public Task<Result<MessageGatewayError>> SendAsync(
        OutboundMessage message,
        CancellationToken cancellationToken)
    {
        LastMessage = message;
        return Task.FromResult(_result);
    }
}
