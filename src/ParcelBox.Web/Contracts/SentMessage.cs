namespace ParcelBox.Web.Contracts;

public sealed record SentMessage(
    Guid Id,
    string Destination,
    string Text,
    DateTimeOffset SentAt);
