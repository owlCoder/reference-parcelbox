namespace ParcelBox.Application.Abstractions;

public interface ILockerController
{
    Task<LockerOpenResult> OpenAsync(
        string lockerCode,
        string compartmentNumber,
        CancellationToken cancellationToken);
}
