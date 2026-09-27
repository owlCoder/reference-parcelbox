using System.Collections.Concurrent;
using ParcelBox.Simulators.MessageGateway.Enums;

namespace ParcelBox.Simulators.MessageGateway.Models;

public sealed class SimulatorState
{
    public MessageGatewayMode Mode { get; set; } = MessageGatewayMode.Normal;
    public ConcurrentQueue<SentMessage> Messages { get; } = new();
}
