using System.Collections.Concurrent;

namespace ParcelBox.Simulators.MessageGateway;

public sealed class SimulatorState
{
    public MessageGatewayMode Mode { get; set; } = MessageGatewayMode.Normal;
    public ConcurrentQueue<SentMessage> Messages { get; } = new();

    public void Clear()
    {
        while (Messages.TryDequeue(out _))
        {
        }
    }
}
