namespace ParcelBox.Web.ViewModels;

public sealed class ActivityEntry
{
    public DateTimeOffset OccurredAt { get; set; }
    public string Message { get; set; } = string.Empty;

    public ActivityEntry()
    {
    }

    public ActivityEntry(DateTimeOffset occurredAt, string message)
    {
        OccurredAt = occurredAt;
        Message = message;
    }
}
