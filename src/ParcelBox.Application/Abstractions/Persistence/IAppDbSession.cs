namespace ParcelBox.Application.Abstractions.Persistence;

public interface IAppDbSession
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
