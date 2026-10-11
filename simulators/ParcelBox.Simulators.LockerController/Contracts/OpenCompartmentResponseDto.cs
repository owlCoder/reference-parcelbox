using ParcelBox.Simulators.LockerController.Enums;

namespace ParcelBox.Simulators.LockerController.Contracts;

public sealed class OpenCompartmentResponseDto
{
    public string LockerCode { get; set; } = string.Empty;
    public string CompartmentNumber { get; set; } = string.Empty;
    public LockerOpenStatus Status { get; set; }

    public OpenCompartmentResponseDto()
    {
    }

    public OpenCompartmentResponseDto(string lockerCode, string compartmentNumber, LockerOpenStatus status)
    {
        LockerCode = lockerCode;
        CompartmentNumber = compartmentNumber;
        Status = status;
    }
}
