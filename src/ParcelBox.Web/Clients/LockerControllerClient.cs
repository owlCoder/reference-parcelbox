using System.Text.Json;
using ParcelBox.Web.Enums;
using ParcelBox.Web.Interfaces;

namespace ParcelBox.Web.Clients;

public sealed class LockerControllerClient : SimulatorClientBase<LockerMode>, ILockerControllerClient
{
    public LockerControllerClient(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        : base(httpClient, jsonOptions)
    {
    }
}
