namespace ParcelBox.Web.Interfaces;

public interface IServiceHealthClient
{
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
