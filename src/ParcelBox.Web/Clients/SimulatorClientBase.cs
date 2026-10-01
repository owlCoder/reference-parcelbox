using System.Text.Json;
using ParcelBox.Web.Contracts;
using ParcelBox.Web.Interfaces;

namespace ParcelBox.Web.Clients;

public abstract class SimulatorClientBase<TMode> : ServiceClientBase, ISimulatorClient<TMode>
    where TMode : struct, Enum
{
    private const string ModePath = "api/simulator/mode";

    protected SimulatorClientBase(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        : base(httpClient)
    {
        JsonOptions = jsonOptions;
    }

    protected JsonSerializerOptions JsonOptions { get; }

    public async Task<TMode?> GetModeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<SimulatorModeResponse<TMode>>(
                ModePath,
                JsonOptions,
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

    public async Task<bool> SetModeAsync(TMode mode, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.PutAsJsonAsync(
                ModePath,
                new SetSimulatorModeRequest<TMode>(mode),
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var confirmed = await response.Content.ReadFromJsonAsync<SimulatorModeResponse<TMode>>(
                JsonOptions,
                cancellationToken);

            return confirmed is not null && EqualityComparer<TMode>.Default.Equals(confirmed.Mode, mode);
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
