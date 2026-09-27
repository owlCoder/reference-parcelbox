namespace ParcelBox.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
