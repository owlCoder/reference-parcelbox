using ParcelBox.Simulators.MessageGateway.Enums;

namespace ParcelBox.Simulators.MessageGateway.Contracts;

public sealed class SimulatorModeResponseDto
{
    public MessageGatewayMode Mode { get; set; }

    public SimulatorModeResponseDto()
    {
    }

    public SimulatorModeResponseDto(MessageGatewayMode mode)
    {
        Mode = mode;
    }
}
