namespace ParcelBox.Application.DTOs.External;

public sealed class OutboundMessageDto
{
    public string Destination { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public OutboundMessageDto()
    {
    }

    public OutboundMessageDto(string destination, string text)
    {
        Destination = destination;
        Text = text;
    }
}
