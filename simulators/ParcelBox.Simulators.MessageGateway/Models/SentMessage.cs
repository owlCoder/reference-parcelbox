namespace ParcelBox.Simulators.MessageGateway.Models;

public sealed class SentMessage
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }

    public SentMessage()
    {
    }

    public SentMessage(Guid id, string destination, string text, DateTimeOffset sentAt)
    {
        Id = id;
        Destination = destination;
        Text = text;
        SentAt = sentAt;
    }
}
