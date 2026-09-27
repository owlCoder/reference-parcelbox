using ParcelBox.Api.Contracts;
using ParcelBox.Api.Extensions;
using ParcelBox.Application.Interfaces.Services;

namespace ParcelBox.Api.Endpoints;

public static class PickupEndpoints
{
    public static IEndpointRouteBuilder MapPickupEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/pickup/{trackingCode}", PickupAsync)
            .WithTags("Pickup");

        return endpoints;
    }

    private static async Task<IResult> PickupAsync(
        string trackingCode,
        PickupRequest request,
        IPickupService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CompletePickupAsync(
            trackingCode,
            request.Code,
            cancellationToken);

        return result.IsFailure
            ? result.Error.ToProblemResult()
            : Results.Ok(new { status = "picked-up" });
    }
}
