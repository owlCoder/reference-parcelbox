namespace ParcelBox.Simulators.MessageGateway.Contracts;

public sealed class SendMessageRequest
{
    public string Destination { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public SendMessageRequest()
    {
    }

    public SendMessageRequest(string destination, string text)
    {
        Destination = destination;
        Text = text;
    }
}
