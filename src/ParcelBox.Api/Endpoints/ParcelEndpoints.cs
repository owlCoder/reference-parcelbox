using ParcelBox.Api.Contracts;
using ParcelBox.Api.Extensions;
using ParcelBox.Application.DTOs.Parcels;
using ParcelBox.Application.Interfaces.Services;

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
        IParcelService service,
        CancellationToken cancellationToken)
    {
        var input = new RegisterParcelInput(
            request.TrackingCode,
            request.RecipientPhone,
            request.Size);

        var result = await service.RegisterAsync(input, cancellationToken);

        return result.IsFailure
            ? result.Error.ToProblemResult()
            : Results.Created($"/api/parcels/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        IParcelService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);

        return result.IsFailure
            ? result.Error.ToProblemResult()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> StoreAsync(
        Guid id,
        IParcelStorageService service,
        CancellationToken cancellationToken)
    {
        var result = await service.StoreAsync(id, cancellationToken);

        return result.IsFailure
            ? result.Error.ToProblemResult()
            : Results.Ok(result.Value);
    }
}
