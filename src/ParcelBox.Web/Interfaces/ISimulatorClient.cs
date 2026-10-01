namespace ParcelBox.Web.Interfaces;

public interface ISimulatorClient<TMode> : IServiceHealthClient
    where TMode : struct, Enum
{
    Task<TMode?> GetModeAsync(CancellationToken cancellationToken = default);

    Task<bool> SetModeAsync(TMode mode, CancellationToken cancellationToken = default);
}
