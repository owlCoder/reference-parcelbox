using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<SimulatorState>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/simulator/mode", (SimulatorState state) => Results.Ok(new { mode = state.Mode }));

app.MapPut("/api/simulator/mode", (SetModeRequest request, SimulatorState state) =>
{
    if (!Enum.TryParse<MessageGatewayMode>(request.Mode, true, out var mode))
        return Results.BadRequest(new { error = "Allowed modes: Normal, Unavailable." });

    state.Mode = mode;
    return Results.Ok(new { mode = state.Mode });
});

app.MapGet("/api/messages", (SimulatorState state) => Results.Ok(state.Messages.ToArray()));

app.MapDelete("/api/messages", (SimulatorState state) =>
{
    state.Clear();
    return Results.NoContent();
});

app.MapPost("/api/messages", (SendMessageRequest request, SimulatorState state) =>
{
    if (state.Mode == MessageGatewayMode.Unavailable)
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

    var message = new SentMessage(Guid.NewGuid(), request.Destination, request.Text, DateTimeOffset.UtcNow);
    state.Messages.Enqueue(message);
    return Results.Accepted("/api/messages", message);
});

app.Run();

public sealed class SimulatorState
{
    public MessageGatewayMode Mode { get; set; } = MessageGatewayMode.Normal;
    public ConcurrentQueue<SentMessage> Messages { get; } = new();

    public void Clear()
    {
        while (Messages.TryDequeue(out _)) { }
    }
}

public enum MessageGatewayMode
{
    Normal,
    Unavailable
}

public sealed record SetModeRequest(string Mode);
public sealed record SendMessageRequest(string Destination, string Text);
public sealed record SentMessage(Guid Id, string Destination, string Text, DateTimeOffset SentAt);
