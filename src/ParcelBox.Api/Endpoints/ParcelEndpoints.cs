using ParcelBox.Api.Contracts;
using ParcelBox.Api.Extensions;
using ParcelBox.Application.Parcels;

namespace ParcelBox.Api.Endpoints;

public static class ParcelEndpoints
{
    public static IEndpointRouteBuilder MapParcelEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/parcels").WithTags("Parcels");

        group.MapPost("/", RegisterAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapPost("/{id:guid}/store", StoreAsync);

        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterParcelRequest request,
        RegisterParcelHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterParcelCommand(
            request.TrackingCode,
            request.RecipientPhone,
            request.Size);

        var result = await handler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemResult();
        }

        return Results.Created($"/api/parcels/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        GetParcelHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemResult();
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> StoreAsync(
        Guid id,
        StoreParcelHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemResult();
        }

        return Results.Ok(result.Value);
    }
}
