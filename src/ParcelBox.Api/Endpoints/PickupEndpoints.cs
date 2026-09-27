using ParcelBox.Api.Contracts;
using ParcelBox.Api.Extensions;
using ParcelBox.Application.Pickup;

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
        PickupParcelHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            trackingCode,
            request.Code,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemResult();
        }

        return Results.Ok(new { status = "picked-up" });
    }
}
