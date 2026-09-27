namespace ParcelBox.Simulators.MessageGateway.Contracts;

public sealed record SendMessageRequest(string Destination, string Text);
