using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Lockers.Models;
using ParcelBox.Domain.Parcels.Models;
using ParcelBox.Domain.Pickup.Models;

namespace ParcelBox.Application.Tests.TestContexts;

internal sealed class PickupTestContext
{
    public PickupService Service { get; set; }
    public Parcel Parcel { get; set; }
    public Compartment Compartment { get; set; }
    public PickupAccess Access { get; set; }
    public UnitOfWorkFake UnitOfWork { get; set; }
    public LockerControllerFake LockerController { get; set; }

    public PickupTestContext(PickupService service, Parcel parcel, Compartment compartment, PickupAccess access, UnitOfWorkFake unitOfWork, LockerControllerFake lockerController)
    {
        Service = service;
        Parcel = parcel;
        Compartment = compartment;
        Access = access;
        UnitOfWork = unitOfWork;
        LockerController = lockerController;
    }
}
