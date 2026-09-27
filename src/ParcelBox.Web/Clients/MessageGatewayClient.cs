using System.Net.Http.Json;
using System.Text.Json;
using ParcelBox.Web.Contracts;

namespace ParcelBox.Web.Clients;

public sealed class MessageGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public MessageGatewayClient(HttpClient httpClient, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient;
        _jsonOptions = jsonOptions;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("health", cancellationToken);
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

    public async Task<MessageGatewayMode?> GetModeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<SimulatorModeResponse<MessageGatewayMode>>(
                "api/simulator/mode",
                _jsonOptions,
                cancellationToken);

            return response?.Mode;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<bool> SetModeAsync(
        MessageGatewayMode mode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PutAsJsonAsync(
                "api/simulator/mode",
                new SetSimulatorModeRequest<MessageGatewayMode>(mode),
                _jsonOptions,
                cancellationToken);

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

    public async Task<IReadOnlyList<SentMessage>> ListMessagesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = await _httpClient.GetFromJsonAsync<List<SentMessage>>(
                "api/messages",
                _jsonOptions,
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
            using var response = await _httpClient.DeleteAsync("api/messages", cancellationToken);
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
