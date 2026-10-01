using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Lockers.Models;
using ParcelBox.Domain.Parcels.Models;

namespace ParcelBox.Application.Tests.TestContexts;

internal sealed record ParcelStorageTestContext(
    ParcelStorageService Service,
    Parcel Parcel,
    Compartment Compartment,
    PickupAccessRepositoryFake PickupAccesses,
    UnitOfWorkFake UnitOfWork,
    MessageGatewayFake MessageGateway);
