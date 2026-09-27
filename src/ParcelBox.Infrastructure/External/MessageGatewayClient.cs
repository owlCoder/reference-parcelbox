using System.Net.Http.Json;
using ParcelBox.Application.Abstractions;

namespace ParcelBox.Infrastructure.External;

internal sealed class MessageGatewayClient(HttpClient httpClient) : IMessageGateway
{
    public async Task<bool> SendPickupCodeAsync(
        string destination,
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/messages",
                new
                {
                    destination,
                    text = $"Parcel {trackingCode} is ready. Pickup code: {pickupCode}"
                },
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
