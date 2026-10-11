namespace ParcelBox.Web.Contracts;

public sealed class SimulatorModeResponse<TMode>
{
    public TMode Mode { get; set; } = default!;

    public SimulatorModeResponse()
    {
    }

    public SimulatorModeResponse(TMode mode)
    {
        Mode = mode;
    }
}
