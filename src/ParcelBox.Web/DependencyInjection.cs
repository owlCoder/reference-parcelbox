using System.Text.Json;
using System.Text.Json.Serialization;
using ParcelBox.Web.Clients;

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

        services.AddHttpClient<ParcelBoxApiClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:ApiBaseUrl"] ?? "http://localhost:5100");
        });

        services.AddHttpClient<LockerControllerClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:LockerControllerBaseUrl"] ?? "http://localhost:5101");
        });

        services.AddHttpClient<MessageGatewayClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:MessageGatewayBaseUrl"] ?? "http://localhost:5102");
        });

        return services;
    }
}
