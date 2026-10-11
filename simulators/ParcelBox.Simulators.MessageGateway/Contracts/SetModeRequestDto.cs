using ParcelBox.Simulators.MessageGateway.Enums;

namespace ParcelBox.Simulators.MessageGateway.Contracts;

public sealed class SetModeRequestDto
{
    public MessageGatewayMode Mode { get; set; }

    public SetModeRequestDto()
    {
    }

    public SetModeRequestDto(MessageGatewayMode mode)
    {
        Mode = mode;
    }
}
