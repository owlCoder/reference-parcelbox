using System.Text.Json;
using System.Text.Json.Serialization;
using ParcelBox.Web.Clients;
using ParcelBox.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
jsonOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
builder.Services.AddSingleton(jsonOptions);

builder.Services.AddHttpClient<ParcelBoxApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:ApiBaseUrl"] ?? "http://localhost:5100");
});

builder.Services.AddHttpClient<LockerSimulatorClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:LockerSimulatorBaseUrl"] ?? "http://localhost:5101");
});

builder.Services.AddHttpClient<MessageGatewayClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:MessageGatewayBaseUrl"] ?? "http://localhost:5102");
});

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
