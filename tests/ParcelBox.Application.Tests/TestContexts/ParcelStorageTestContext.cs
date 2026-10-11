using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Lockers.Models;
using ParcelBox.Domain.Parcels.Models;

namespace ParcelBox.Application.Tests.TestContexts;

internal sealed class ParcelStorageTestContext
{
    public ParcelStorageService Service { get; set; }
    public Parcel Parcel { get; set; }
    public Compartment Compartment { get; set; }
    public PickupAccessRepositoryFake PickupAccesses { get; set; }
    public UnitOfWorkFake UnitOfWork { get; set; }
    public MessageGatewayFake MessageGateway { get; set; }

    public ParcelStorageTestContext(ParcelStorageService service, Parcel parcel, Compartment compartment, PickupAccessRepositoryFake pickupAccesses, UnitOfWorkFake unitOfWork, MessageGatewayFake messageGateway)
    {
        Service = service;
        Parcel = parcel;
        Compartment = compartment;
        PickupAccesses = pickupAccesses;
        UnitOfWork = unitOfWork;
        MessageGateway = messageGateway;
    }
}
