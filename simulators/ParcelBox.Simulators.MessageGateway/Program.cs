using System.Text.Json.Serialization;
using ParcelBox.Simulators.MessageGateway;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(allowIntegerValues: false));
});
builder.Services.AddSingleton<SimulatorState>();
builder.Services.AddSingleton(TimeProvider.System);

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

app.MapGet("/api/messages", (SimulatorState state) => Results.Ok(state.Messages.ToArray()));

app.MapDelete("/api/messages", (SimulatorState state) =>
{
    state.Clear();
    return Results.NoContent();
});

app.MapPost("/api/messages", (
    SendMessageRequest request,
    SimulatorState state,
    TimeProvider timeProvider) =>
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
});

app.Run();
