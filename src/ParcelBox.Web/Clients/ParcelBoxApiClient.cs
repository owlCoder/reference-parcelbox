using System.Text.Json;
using ParcelBox.Web.Common.Results;
using ParcelBox.Web.Contracts;
using ParcelBox.Web.Interfaces;

namespace ParcelBox.Web.Clients;

public sealed class ParcelBoxApiClient : ServiceClientBase, IParcelBoxApiClient
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ParcelBoxApiClient(HttpClient httpClient, JsonSerializerOptions jsonOptions) : base(httpClient)
    {
        _jsonOptions = jsonOptions;
    }

    public Task<ClientResult<ParcelDetailsDto>> RegisterAsync(RegisterParcelRequestDto request, CancellationToken cancellationToken = default)
    {
        return SendAndReadAsync<ParcelDetailsDto>(() => HttpClient.PostAsJsonAsync("api/parcels", request, _jsonOptions, cancellationToken), cancellationToken);
    }

    public Task<ClientResult<ParcelDetailsDto>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return SendAndReadAsync<ParcelDetailsDto>(() => HttpClient.GetAsync($"api/parcels/{id}", cancellationToken), cancellationToken);
    }

    public Task<ClientResult<StoreParcelResultDto>> StoreAsync(Guid parcelId, CancellationToken cancellationToken = default)
    {
        return SendAndReadAsync<StoreParcelResultDto>(() => HttpClient.PostAsync($"api/parcels/{parcelId}/store", null, cancellationToken), cancellationToken);
    }

    public Task<ClientResult<List<CompartmentDetailsDto>>> ListCompartmentsAsync(CancellationToken cancellationToken = default)
    {
        return SendAndReadAsync<List<CompartmentDetailsDto>>(() => HttpClient.GetAsync("api/lockers/compartments", cancellationToken), cancellationToken);
    }

    public async Task<ClientResult> PickupAsync(string trackingCode, string pickupCode, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.PostAsJsonAsync($"api/pickup/{Uri.EscapeDataString(trackingCode)}", new PickupRequestDto(pickupCode), _jsonOptions, cancellationToken);

            return response.IsSuccessStatusCode
                ? ClientResult.Success()
                : ClientResult.Failure(await ReadErrorAsync(response, cancellationToken));
        }
        catch (HttpRequestException)
        {
            return ClientResult.Failure("ParcelBox API is unavailable.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ClientResult.Failure("ParcelBox API request timed out.");
        }
    }

    private async Task<ClientResult<T>> SendAndReadAsync<T>(Func<Task<HttpResponseMessage>> request, CancellationToken cancellationToken) where T : class, new()
    {
        try
        {
            using var response = await request();

            if (!response.IsSuccessStatusCode)
            {
                return ClientResult<T>.Failure(await ReadErrorAsync(response, cancellationToken));
            }

            var value = await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);

            return value is null
                ? ClientResult<T>.Failure("ParcelBox API returned an empty response.")
                : ClientResult<T>.Success(value);
        }
        catch (HttpRequestException)
        {
            return ClientResult<T>.Failure("ParcelBox API is unavailable.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ClientResult<T>.Failure("ParcelBox API request timed out.");
        }
        catch (JsonException)
        {
            return ClientResult<T>.Failure("ParcelBox API returned an invalid response.");
        }
    }

    private async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponseDto>(_jsonOptions, cancellationToken);

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
