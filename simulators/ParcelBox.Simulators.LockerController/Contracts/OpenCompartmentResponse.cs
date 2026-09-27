namespace ParcelBox.Simulators.LockerController;

public sealed record OpenCompartmentResponse(
    string LockerCode,
    string CompartmentNumber,
    LockerOpenStatus Status);
