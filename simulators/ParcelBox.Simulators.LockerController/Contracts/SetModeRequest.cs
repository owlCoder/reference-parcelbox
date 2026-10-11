using ParcelBox.Simulators.LockerController.Enums;

namespace ParcelBox.Simulators.LockerController.Contracts;

public sealed class SetModeRequest
{
    public LockerMode Mode { get; set; }

    public SetModeRequest()
    {
    }

    public SetModeRequest(LockerMode mode)
    {
        Mode = mode;
    }
}
