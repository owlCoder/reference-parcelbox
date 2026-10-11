namespace ParcelBox.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken);
}
