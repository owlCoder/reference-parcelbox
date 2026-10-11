namespace ParcelBox.Simulators.MessageGateway.Contracts;

public sealed class SendMessageRequestDto
{
    public string Destination { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public SendMessageRequestDto()
    {
    }

    public SendMessageRequestDto(string destination, string text)
    {
        Destination = destination;
        Text = text;
    }
}
