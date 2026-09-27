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
        if (string.IsNullOrWhiteSpace(trackingCode) || string.IsNullOrWhiteSpace(request.Code))
            return Results.BadRequest(new { error = "Tracking code and pickup code are required." });

        var result = await handler.HandleAsync(trackingCode, request.Code, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { status = "picked-up" })
            : Results.BadRequest(new { error = result.Error });
    }
}

public sealed record PickupRequest(string Code);
