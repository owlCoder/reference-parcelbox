using ParcelBox.Api.Endpoints;
using ParcelBox.Application.Lockers;
using ParcelBox.Application.Parcels;
using ParcelBox.Application.Pickup;
using ParcelBox.Infrastructure;
using ParcelBox.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddScoped<RegisterParcelHandler>();
builder.Services.AddScoped<GetParcelHandler>();
builder.Services.AddScoped<StoreParcelHandler>();
builder.Services.AddScoped<ListCompartmentsHandler>();
builder.Services.AddScoped<PickupParcelHandler>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapParcelEndpoints();
app.MapLockerEndpoints();
app.MapPickupEndpoints();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParcelBoxDbContext>();
    var initializationResult = await DatabaseInitializer.InitializeAsync(db);

    if (initializationResult.IsFailure)
    {
        app.Logger.LogCritical(
            "Database initialization failed: {ErrorCode} - {ErrorMessage}",
            initializationResult.Error.Code,
            initializationResult.Error.Message);

        return;
    }
}

await app.RunAsync();
