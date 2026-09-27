using System.Net;
using ParcelBox.Application.Common.Results;
using ParcelBox.Application.Interfaces.External;

namespace ParcelBox.Infrastructure.External;

internal sealed class LockerControllerClient : ILockerController
{
    private readonly HttpClient _httpClient;

    public LockerControllerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<LockerControllerError>> OpenAsync(
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
                return Result<LockerControllerError>.Success();
            }

            return response.StatusCode == HttpStatusCode.Conflict
                ? Result<LockerControllerError>.Failure(LockerControllerError.Jammed)
                : Result<LockerControllerError>.Failure(LockerControllerError.Unavailable);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<LockerControllerError>.Failure(LockerControllerError.Unavailable);
        }
        catch (HttpRequestException)
        {
            return Result<LockerControllerError>.Failure(LockerControllerError.Unavailable);
        }
    }
}
