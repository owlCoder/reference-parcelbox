using ParcelBox.Simulators.LockerController.Enums;

namespace ParcelBox.Simulators.LockerController.Contracts;

public sealed class SimulatorModeResponse
{
    public LockerMode Mode { get; set; }

    public SimulatorModeResponse()
    {
    }

    public SimulatorModeResponse(LockerMode mode)
    {
        Mode = mode;
    }
}
