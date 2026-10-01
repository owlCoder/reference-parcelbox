using System.Text.Json;
using System.Text.Json.Serialization;
using ParcelBox.Web.Clients;
using ParcelBox.Web.Interfaces;

namespace ParcelBox.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWebClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        services.AddSingleton(jsonOptions);

        services.AddHttpClient<IParcelBoxApiClient, ParcelBoxApiClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:ApiBaseUrl"] ?? "http://localhost:5100");
        });

        services.AddHttpClient<ILockerControllerClient, LockerControllerClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:LockerControllerBaseUrl"] ?? "http://localhost:5101");
        });

        services.AddHttpClient<IMessageGatewayClient, MessageGatewayClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:MessageGatewayBaseUrl"] ?? "http://localhost:5102");
        });

        return services;
    }
}
