namespace ParcelBox.Application.Interfaces.External;

public sealed record OutboundMessage(string Destination, string Text);
