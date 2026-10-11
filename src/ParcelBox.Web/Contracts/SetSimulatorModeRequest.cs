namespace ParcelBox.Web.Contracts;

public sealed class SetSimulatorModeRequest<TMode>
{
    public TMode Mode { get; set; } = default!;

    public SetSimulatorModeRequest()
    {
    }

    public SetSimulatorModeRequest(TMode mode)
    {
        Mode = mode;
    }
}
