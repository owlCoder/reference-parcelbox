using ParcelBox.Simulators.MessageGateway.Contracts;
using ParcelBox.Simulators.MessageGateway.Enums;
using ParcelBox.Simulators.MessageGateway.Models;

namespace ParcelBox.Simulators.MessageGateway.Endpoints;

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/messages", GetMessages);
        endpoints.MapDelete("/api/messages", ClearMessages);
        endpoints.MapPost("/api/messages", SendMessage);

        return endpoints;
    }

    private static IResult GetMessages(SimulatorState state)
    {
        return Results.Ok(state.Messages.ToArray());
    }

    private static IResult ClearMessages(SimulatorState state)
    {
        while (state.Messages.TryDequeue(out _))
        {
        }

        return Results.NoContent();
    }

    private static IResult SendMessage(
        SendMessageRequest request,
        SimulatorState state,
        TimeProvider timeProvider)
    {
        if (state.Mode == MessageGatewayMode.Unavailable)
        {
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var message = new SentMessage(
            Guid.NewGuid(),
            request.Destination,
            request.Text,
            timeProvider.GetUtcNow());

        state.Messages.Enqueue(message);
        return Results.Accepted("/api/messages", message);
    }
}
