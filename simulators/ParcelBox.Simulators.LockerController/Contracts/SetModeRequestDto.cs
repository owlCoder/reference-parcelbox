using ParcelBox.Simulators.LockerController.Enums;

namespace ParcelBox.Simulators.LockerController.Contracts;

public sealed class SetModeRequestDto
{
    public LockerMode Mode { get; set; }

    public SetModeRequestDto()
    {
    }

    public SetModeRequestDto(LockerMode mode)
    {
        Mode = mode;
    }
}
