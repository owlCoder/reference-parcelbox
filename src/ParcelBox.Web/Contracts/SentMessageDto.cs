namespace ParcelBox.Web.Contracts;

public sealed class SentMessageDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }

    public SentMessageDto()
    {
    }

    public SentMessageDto(Guid id, string destination, string text, DateTimeOffset sentAt)
    {
        Id = id;
        Destination = destination;
        Text = text;
        SentAt = sentAt;
    }
}
