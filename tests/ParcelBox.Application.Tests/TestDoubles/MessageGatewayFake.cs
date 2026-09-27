using ParcelBox.Application.Common.Results;
using ParcelBox.Application.Interfaces.External;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class MessageGatewayFake : IMessageGateway
{
    public MessageGatewayError Error { get; set; }
    public OutboundMessage? LastMessage { get; private set; }

    public Task<Result<MessageGatewayError>> SendAsync(
        OutboundMessage message,
        CancellationToken cancellationToken)
    {
        LastMessage = message;

        var result = Error == MessageGatewayError.None
            ? Result<MessageGatewayError>.Success()
            : Result<MessageGatewayError>.Failure(Error);

        return Task.FromResult(result);
    }
}
