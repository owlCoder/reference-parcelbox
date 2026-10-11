using ParcelBox.Simulators.MessageGateway.Enums;

namespace ParcelBox.Simulators.MessageGateway.Contracts;

public sealed class SetModeRequest
{
    public MessageGatewayMode Mode { get; set; };

    public SetModeRequest()
    {
    }

    public SetModeRequest(MessageGatewayMode mode)
    {
        Mode = mode;
    }
}
