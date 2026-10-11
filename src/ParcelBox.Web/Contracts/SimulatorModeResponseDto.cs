namespace ParcelBox.Web.Contracts;

public sealed class SimulatorModeResponseDto<TMode>
{
    public TMode Mode { get; set; } = default!;

    public SimulatorModeResponseDto()
    {
    }

    public SimulatorModeResponseDto(TMode mode)
    {
        Mode = mode;
    }
}
