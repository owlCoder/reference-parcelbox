using ParcelBox.Simulators.MessageGateway.Enums;

namespace ParcelBox.Simulators.MessageGateway.Contracts;

public sealed class SimulatorModeResponse
{
    public MessageGatewayMode Mode { get; set; }

    public SimulatorModeResponse()
    {
    }

    public SimulatorModeResponse(MessageGatewayMode mode)
    {
        Mode = mode;
    }
}
