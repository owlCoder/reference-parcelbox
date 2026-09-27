using ParcelBox.Application.Parcels;
using ParcelBox.Domain.Parcels;

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
        if (string.IsNullOrWhiteSpace(request.TrackingCode) || string.IsNullOrWhiteSpace(request.RecipientPhone))
            return Results.BadRequest(new { error = "Tracking code and recipient phone are required." });

        if (!Enum.IsDefined(request.Size))
            return Results.BadRequest(new { error = "Invalid parcel size." });

        var result = await handler.HandleAsync(
            new RegisterParcelCommand(request.TrackingCode, request.RecipientPhone, request.Size),
            cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/parcels/{result.Value!.Id}", result.Value)
            : Results.BadRequest(new { error = result.Error });
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        GetParcelHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(new { error = result.Error });
    }

    private static async Task<IResult> StoreAsync(
        Guid id,
        StoreParcelHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error });
    }
}

public sealed record RegisterParcelRequest(string TrackingCode, string RecipientPhone, ParcelSize Size);
