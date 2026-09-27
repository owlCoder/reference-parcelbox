using ParcelBox.Simulators.LockerController.Enums;

namespace ParcelBox.Simulators.LockerController.Contracts;

public sealed record OpenCompartmentResponse(
    string LockerCode,
    string CompartmentNumber,
    LockerOpenStatus Status);
