using System.Net;
using ParcelBox.Application.Abstractions.External;
using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Infrastructure.External;

internal sealed class LockerControllerClient : ILockerController
{
    private readonly HttpClient _httpClient;

    public LockerControllerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result> OpenAsync(
        string lockerCode,
        string compartmentNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PostAsync(
                $"api/compartments/{Uri.EscapeDataString(lockerCode)}/{Uri.EscapeDataString(compartmentNumber)}/open",
                content: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return Result.Failure(LockerControllerErrors.Jammed);
            }

            return Result.Failure(LockerControllerErrors.Unavailable);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure(LockerControllerErrors.Unavailable);
        }
        catch (HttpRequestException)
        {
            return Result.Failure(LockerControllerErrors.Unavailable);
        }
    }
}
