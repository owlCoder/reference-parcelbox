using ParcelBox.Simulators.MessageGateway.Contracts;
using ParcelBox.Simulators.MessageGateway.Enums;
using ParcelBox.Simulators.MessageGateway.Models;

namespace ParcelBox.Simulators.MessageGateway.Endpoints;

public static class SimulatorEndpoints
{
    public static IEndpointRouteBuilder MapSimulatorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", GetHealth);
        endpoints.MapGet("/api/simulator/mode", GetMode);
        endpoints.MapPut("/api/simulator/mode", SetMode);

        return endpoints;
    }

    private static IResult GetHealth(SimulatorState state)
    {
        return state.Mode == MessageGatewayMode.Unavailable
            ? Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
            : Results.Ok(new { status = "ok" });
    }

    private static IResult GetMode(SimulatorState state)
    {
        return Results.Ok(new SimulatorModeResponse(state.Mode));
    }

    private static IResult SetMode(SetModeRequest request, SimulatorState state)
    {
        state.Mode = request.Mode;
        return Results.Ok(new SimulatorModeResponse(state.Mode));
    }
}
