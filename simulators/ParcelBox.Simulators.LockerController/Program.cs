using ParcelBox.Simulators.LockerController;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<SimulatorState>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/simulator/mode", (SimulatorState state) => Results.Ok(new { mode = state.Mode }));

app.MapPut("/api/simulator/mode", (SetModeRequest request, SimulatorState state) =>
{
    if (!Enum.TryParse<LockerMode>(request.Mode, true, out var mode))
        return Results.BadRequest(new { error = "Allowed modes: Normal, Jammed, Unavailable." });

    state.Mode = mode;
    return Results.Ok(new { mode = state.Mode });
});

app.MapPost("/api/compartments/{lockerCode}/{number}/open", (
    string lockerCode,
    string number,
    SimulatorState state) => state.Mode switch
{
    LockerMode.Normal => Results.Ok(new { lockerCode, number, status = "Opened" }),
    LockerMode.Jammed => Results.Conflict(new { lockerCode, number, status = "Jammed" }),
    _ => Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
});

app.Run();
