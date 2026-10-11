using ParcelBox.Simulators.LockerController.Enums;

namespace ParcelBox.Simulators.LockerController.Contracts;

public sealed class SimulatorModeResponseDto
{
    public LockerMode Mode { get; set; }

    public SimulatorModeResponseDto()
    {
    }

    public SimulatorModeResponseDto(LockerMode mode)
    {
        Mode = mode;
    }
}
