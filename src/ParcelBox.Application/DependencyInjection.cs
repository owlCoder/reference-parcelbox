using Microsoft.Extensions.DependencyInjection;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Application.Services;

namespace ParcelBox.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IParcelService, ParcelService>();
        services.AddScoped<IParcelStorageService, ParcelStorageService>();
        services.AddScoped<ICompartmentService, CompartmentService>();
        services.AddScoped<ILockerService, LockerService>();
        services.AddScoped<IPickupAccessService, PickupAccessService>();
        services.AddScoped<IPickupService, PickupService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
