namespace ParcelBox.Simulators.MessageGateway;

public sealed record SendMessageRequest(string Destination, string Text);
