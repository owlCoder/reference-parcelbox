using System.Net;
using ParcelBox.Application.Abstractions;

namespace ParcelBox.Infrastructure.External;

internal sealed class LockerControllerClient(HttpClient httpClient) : ILockerController
{
    public async Task<LockerOpenResult> OpenAsync(string lockerCode, string compartmentNumber, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsync(
                $"api/compartments/{Uri.EscapeDataString(lockerCode)}/{Uri.EscapeDataString(compartmentNumber)}/open",
                content: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
                return LockerOpenResult.Opened;

            return response.StatusCode == HttpStatusCode.Conflict
                ? LockerOpenResult.Jammed
                : LockerOpenResult.Unavailable;
        }
        catch (HttpRequestException)
        {
            return LockerOpenResult.Unavailable;
        }
    }
}
