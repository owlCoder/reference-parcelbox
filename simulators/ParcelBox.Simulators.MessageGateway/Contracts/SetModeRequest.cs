using ParcelBox.Simulators.MessageGateway.Enums;

namespace ParcelBox.Simulators.MessageGateway.Contracts;

public sealed record SetModeRequest(MessageGatewayMode Mode);
