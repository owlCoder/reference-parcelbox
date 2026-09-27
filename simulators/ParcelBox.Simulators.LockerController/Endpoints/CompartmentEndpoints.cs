using ParcelBox.Simulators.LockerController.Contracts;
using ParcelBox.Simulators.LockerController.Enums;
using ParcelBox.Simulators.LockerController.Models;

namespace ParcelBox.Simulators.LockerController.Endpoints;

public static class CompartmentEndpoints
{
    public static IEndpointRouteBuilder MapCompartmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/compartments/{lockerCode}/{number}/open",
            OpenCompartment);

        return endpoints;
    }

    private static IResult OpenCompartment(
        string lockerCode,
        string number,
        SimulatorState state)
    {
        return state.Mode switch
        {
            LockerMode.Normal => Results.Ok(
                new OpenCompartmentResponse(lockerCode, number, LockerOpenStatus.Opened)),
            LockerMode.Jammed => Results.Conflict(
                new OpenCompartmentResponse(lockerCode, number, LockerOpenStatus.Jammed)),
            _ => Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
        };
    }
}
