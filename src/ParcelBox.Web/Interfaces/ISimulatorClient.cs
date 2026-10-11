namespace ParcelBox.Web.Interfaces;

public interface ISimulatorClient<TMode> : IServiceHealthClient
    where TMode : struct, Enum
{
    Task<(bool IsSuccess, TMode Value)> GetModeAsync(CancellationToken cancellationToken = default);

    Task<bool> SetModeAsync(TMode mode, CancellationToken cancellationToken = default);
}
