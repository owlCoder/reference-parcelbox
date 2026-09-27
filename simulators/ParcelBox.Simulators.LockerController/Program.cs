using System.Text.Json.Serialization;
using ParcelBox.Simulators.LockerController.Endpoints;
using ParcelBox.Simulators.LockerController.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(allowIntegerValues: false));
});
builder.Services.AddSingleton<SimulatorState>();

var app = builder.Build();

app.MapSimulatorEndpoints();
app.MapCompartmentEndpoints();

app.Run();
