using System.Net.Http.Json;
using ParcelBox.Application.Abstractions.External;
using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Infrastructure.External;

internal sealed class MessageGatewayClient : IMessageGateway
{
    private readonly HttpClient _httpClient;

    public MessageGatewayClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<MessageGatewayError>> SendPickupCodeAsync(
        string destination,
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new
            {
                destination,
                text = $"Parcel {trackingCode} is ready. Pickup code: {pickupCode}"
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "api/messages",
                request,
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
