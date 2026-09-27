using ParcelBox.Simulators.LockerController.Enums;

namespace ParcelBox.Simulators.LockerController.Models;

public sealed class SimulatorState
{
    public LockerMode Mode { get; set; } = LockerMode.Normal;
}
