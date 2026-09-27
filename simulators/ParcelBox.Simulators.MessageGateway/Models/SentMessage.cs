namespace ParcelBox.Simulators.MessageGateway.Models;

public sealed record SentMessage(
    Guid Id,
    string Destination,
    string Text,
    DateTimeOffset SentAt);
