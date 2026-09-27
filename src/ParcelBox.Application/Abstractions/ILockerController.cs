namespace ParcelBox.Application.Abstractions;

public interface ILockerController
{
    Task<LockerOpenResult> OpenAsync(string lockerCode, string compartmentNumber, CancellationToken cancellationToken);
}

public enum LockerOpenResult
{
    Opened = 1,
    Jammed = 2,
    Unavailable = 3
}
