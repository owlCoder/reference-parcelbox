namespace ParcelBox.Application.Abstractions.External;

public sealed record OutboundMessage(
    string Destination,
    string Text);
