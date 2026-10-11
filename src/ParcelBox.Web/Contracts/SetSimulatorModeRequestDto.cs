namespace ParcelBox.Web.Contracts;

public sealed class SetSimulatorModeRequestDto<TMode>
{
    public TMode Mode { get; set; } = default!;

    public SetSimulatorModeRequestDto()
    {
    }

    public SetSimulatorModeRequestDto(TMode mode)
    {
        Mode = mode;
    }
}
