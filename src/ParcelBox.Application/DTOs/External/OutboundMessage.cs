namespace ParcelBox.Application.DTOs.External;

public sealed class OutboundMessage
{
    public string Destination { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public OutboundMessage()
    {
    }

    public OutboundMessage(string destination, string text)
    {
        Destination = destination;
        Text = text;
    }
}
