using System.Net.Http.Json;
using ParcelBox.Application.Common.Results;
using ParcelBox.Application.Interfaces.External;

namespace ParcelBox.Infrastructure.External;

internal sealed class MessageGatewayClient : IMessageGateway
{
    private readonly HttpClient _httpClient;

    public MessageGatewayClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<MessageGatewayError>> SendAsync(
        OutboundMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/messages",
                message,
                cancellationToken);

            return response.IsSuccessStatusCode
                ? Result<MessageGatewayError>.Success()
                : Result<MessageGatewayError>.Failure(MessageGatewayError.Unavailable);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<MessageGatewayError>.Failure(MessageGatewayError.Unavailable);
        }
        catch (HttpRequestException)
        {
            return Result<MessageGatewayError>.Failure(MessageGatewayError.Unavailable);
        }
    }
}
