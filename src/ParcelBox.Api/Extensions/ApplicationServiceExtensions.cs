using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Application.Services;

namespace ParcelBox.Api.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IParcelService, ParcelService>();
        services.AddScoped<ILockerService, LockerService>();
        services.AddScoped<IPickupAccessService, PickupAccessService>();
        services.AddScoped<IPickupService, PickupService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
