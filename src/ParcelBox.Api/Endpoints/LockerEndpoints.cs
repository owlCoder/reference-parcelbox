using ParcelBox.Application.Interfaces.Services;

namespace ParcelBox.Api.Endpoints;

public static class LockerEndpoints
{
    public static IEndpointRouteBuilder MapLockerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/lockers/compartments", GetCompartmentsAsync)
            .WithTags("Lockers");

        return endpoints;
    }

    private static async Task<IResult> GetCompartmentsAsync(
        ICompartmentService service,
        CancellationToken cancellationToken)
    {
        var compartments = await service.GetAllAsync(cancellationToken);
        return Results.Ok(compartments);
    }
}
