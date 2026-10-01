using System.Text.Json;
using ParcelBox.Web.Contracts;
using ParcelBox.Web.Interfaces;

namespace ParcelBox.Web.Clients;

public sealed class ParcelBoxApiClient : ServiceClientBase, IParcelBoxApiClient
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ParcelBoxApiClient(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        : base(httpClient)
    {
        _jsonOptions = jsonOptions;
    }

    public async Task<(ParcelDetails? Value, string? Error)> RegisterAsync(
        RegisterParcelRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.PostAsJsonAsync(
                "api/parcels",
                request,
                _jsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return (null, await ReadErrorAsync(response, cancellationToken));
            }

            var value = await response.Content.ReadFromJsonAsync<ParcelDetails>(
                _jsonOptions,
                cancellationToken);

            return value is null
                ? (null, "API returned an empty response.")
                : (value, null);
        }
        catch (HttpRequestException)
        {
            return (null, "ParcelBox API is unavailable.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (null, "ParcelBox API request timed out.");
        }
        catch (JsonException)
        {
            return (null, "ParcelBox API returned an invalid response.");
        }
    }

    public async Task<(ParcelDetails? Value, string? Error)> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.GetAsync($"api/parcels/{id}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return (null, await ReadErrorAsync(response, cancellationToken));
            }

            var value = await response.Content.ReadFromJsonAsync<ParcelDetails>(
                _jsonOptions,
                cancellationToken);

            return value is null
                ? (null, "API returned an empty response.")
                : (value, null);
        }
        catch (HttpRequestException)
        {
            return (null, "ParcelBox API is unavailable.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (null, "ParcelBox API request timed out.");
        }
        catch (JsonException)
        {
            return (null, "ParcelBox API returned an invalid response.");
        }
    }

    public async Task<(StoreParcelResult? Value, string? Error)> StoreAsync(
        Guid parcelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.PostAsync(
                $"api/parcels/{parcelId}/store",
                null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return (null, await ReadErrorAsync(response, cancellationToken));
            }

            var value = await response.Content.ReadFromJsonAsync<StoreParcelResult>(
                _jsonOptions,
                cancellationToken);

            return value is null
                ? (null, "API returned an empty response.")
                : (value, null);
        }
        catch (HttpRequestException)
        {
            return (null, "ParcelBox API is unavailable.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (null, "ParcelBox API request timed out.");
        }
        catch (JsonException)
        {
            return (null, "ParcelBox API returned an invalid response.");
        }
    }

    public async Task<(IReadOnlyList<CompartmentDetails>? Value, string? Error)> ListCompartmentsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.GetAsync("api/lockers/compartments", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return (null, await ReadErrorAsync(response, cancellationToken));
            }

            var value = await response.Content.ReadFromJsonAsync<List<CompartmentDetails>>(
                _jsonOptions,
                cancellationToken);

            return (value ?? [], null);
        }
        catch (HttpRequestException)
        {
            return (null, "ParcelBox API is unavailable.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (null, "ParcelBox API request timed out.");
        }
        catch (JsonException)
        {
            return (null, "ParcelBox API returned an invalid response.");
        }
    }

    public async Task<string?> PickupAsync(
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.PostAsJsonAsync(
                $"api/pickup/{Uri.EscapeDataString(trackingCode)}",
                new PickupRequest(pickupCode),
                _jsonOptions,
                cancellationToken);

            return response.IsSuccessStatusCode
                ? null
                : await ReadErrorAsync(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return "ParcelBox API is unavailable.";
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return "ParcelBox API request timed out.";
        }
    }

    private async Task<string> ReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(
                _jsonOptions,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(problem?.Detail))
            {
                return problem.Detail;
            }

            if (!string.IsNullOrWhiteSpace(problem?.Title))
            {
                return problem.Title;
            }
        }
        catch (JsonException)
        {
        }

        return response.ReasonPhrase ?? $"HTTP {(int)response.StatusCode}";
    }
}
