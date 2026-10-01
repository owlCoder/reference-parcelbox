using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Lockers.Models;
using ParcelBox.Domain.Parcels.Models;
using ParcelBox.Domain.Pickup.Models;

namespace ParcelBox.Application.Tests.TestContexts;

internal sealed record PickupTestContext(
    PickupService Service,
    Parcel Parcel,
    Compartment Compartment,
    PickupAccess Access,
    UnitOfWorkFake UnitOfWork,
    LockerControllerFake LockerController);
