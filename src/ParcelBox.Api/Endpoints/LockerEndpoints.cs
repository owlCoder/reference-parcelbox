using ParcelBox.Application.Interfaces.Services;

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
        ILockerService service,
        CancellationToken cancellationToken)
    {
        var compartments = await service.GetCompartmentsAsync(cancellationToken);
        return Results.Ok(compartments);
    }
}
