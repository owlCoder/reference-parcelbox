using ParcelBox.Application.Lockers;

namespace ParcelBox.Api.Endpoints;

public static class LockerEndpoints
{
    public static IEndpointRouteBuilder MapLockerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/compartments", async (
            ListCompartmentsHandler handler,
            CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .WithTags("Lockers");

        return endpoints;
    }
}
