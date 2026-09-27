using System.Text.Json.Serialization;
using ParcelBox.Simulators.LockerController;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(allowIntegerValues: false));
});
builder.Services.AddSingleton<SimulatorState>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet(
    "/api/simulator/mode",
    (SimulatorState state) => Results.Ok(new SimulatorModeResponse(state.Mode)));

app.MapPut("/api/simulator/mode", (SetModeRequest request, SimulatorState state) =>
{
    state.Mode = request.Mode;
    return Results.Ok(new SimulatorModeResponse(state.Mode));
});

app.MapPost("/api/compartments/{lockerCode}/{number}/open", (
    string lockerCode,
    string number,
    SimulatorState state) =>
{
    return state.Mode switch
    {
        LockerMode.Normal => Results.Ok(
            new OpenCompartmentResponse(lockerCode, number, LockerOpenStatus.Opened)),
        LockerMode.Jammed => Results.Conflict(
            new OpenCompartmentResponse(lockerCode, number, LockerOpenStatus.Jammed)),
        _ => Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
    };
});

app.Run();
