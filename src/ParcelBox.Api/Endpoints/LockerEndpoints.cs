using ParcelBox.Application.Lockers;

namespace ParcelBox.Api.Endpoints;

public static class LockerEndpoints
{
    public static IEndpointRouteBuilder MapLockerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/compartments", ListAsync)
            .WithTags("Lockers");

        return endpoints;
    }

    private static async Task<IResult> ListAsync(
        ListCompartmentsHandler handler,
        CancellationToken cancellationToken)
    {
        var compartments = await handler.HandleAsync(cancellationToken);
        return Results.Ok(compartments);
    }
}
