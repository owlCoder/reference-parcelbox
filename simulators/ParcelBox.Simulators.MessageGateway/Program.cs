using System.Text.Json.Serialization;
using ParcelBox.Simulators.MessageGateway.Endpoints;
using ParcelBox.Simulators.MessageGateway.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(allowIntegerValues: false));
});
builder.Services.AddSingleton<SimulatorState>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.MapSimulatorEndpoints();
app.MapMessageEndpoints();

app.Run();
