using ParcelBox.Web.Contracts;
using ParcelBox.Web.Enums;

namespace ParcelBox.Web.Interfaces;

public interface IMessageGatewayClient : ISimulatorClient<MessageGatewayMode>
{
    Task<IReadOnlyList<SentMessage>> ListMessagesAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ClearMessagesAsync(CancellationToken cancellationToken = default);
}
