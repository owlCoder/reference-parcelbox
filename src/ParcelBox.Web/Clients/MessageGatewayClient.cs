using System.Text.Json;
using ParcelBox.Web.Contracts;
using ParcelBox.Web.Enums;
using ParcelBox.Web.Interfaces;

namespace ParcelBox.Web.Clients;

public sealed class MessageGatewayClient : SimulatorClientBase<MessageGatewayMode>, IMessageGatewayClient
{
    public MessageGatewayClient(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        : base(httpClient, jsonOptions)
    {
    }

    public async Task<IReadOnlyList<SentMessage>> ListMessagesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = await HttpClient.GetFromJsonAsync<List<SentMessage>>(
                "api/messages",
                JsonOptions,
                cancellationToken);

            return messages ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public async Task<bool> ClearMessagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.DeleteAsync("api/messages", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }
}
