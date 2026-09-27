namespace ParcelBox.Simulators.MessageGateway;

public sealed record SentMessage(
    Guid Id,
    string Destination,
    string Text,
    DateTimeOffset SentAt);
