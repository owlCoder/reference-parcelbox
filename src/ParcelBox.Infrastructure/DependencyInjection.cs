using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParcelBox.Application.Abstractions.External;
using ParcelBox.Application.Abstractions.Persistence;
using ParcelBox.Application.Abstractions.Security;
using ParcelBox.Infrastructure.External;
using ParcelBox.Infrastructure.Persistence;
using ParcelBox.Infrastructure.Security;

namespace ParcelBox.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ParcelBoxDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("ParcelBox")
                ?? "Data Source=parcelbox.db";

            options.UseSqlite(connectionString);
        });

        services.AddScoped<IAppDbSession>(serviceProvider =>
            serviceProvider.GetRequiredService<ParcelBoxDbContext>());
        services.AddScoped<IParcelRepository, ParcelRepository>();
        services.AddScoped<ICompartmentRepository, CompartmentRepository>();
        services.AddScoped<IPickupAccessRepository, PickupAccessRepository>();
        services.AddSingleton<IPickupCodeService, PickupCodeService>();
        services.AddSingleton(TimeProvider.System);

        var lockerUrl = configuration["ExternalServices:LockerController"]
            ?? "http://localhost:5101";

        services.AddHttpClient<ILockerController, LockerControllerClient>(client =>
        {
            client.BaseAddress = new Uri(lockerUrl);
        });

        var messageUrl = configuration["ExternalServices:MessageGateway"]
            ?? "http://localhost:5102";

        services.AddHttpClient<IMessageGateway, MessageGatewayClient>(client =>
        {
            client.BaseAddress = new Uri(messageUrl);
        });

        return services;
    }
}
