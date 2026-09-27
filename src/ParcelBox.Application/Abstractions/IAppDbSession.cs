namespace ParcelBox.Application.Abstractions;

public interface IAppDbSession
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
