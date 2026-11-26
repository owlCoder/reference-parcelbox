using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Lockers.Models;
using ParcelBox.Domain.Parcels.Models;
using ParcelBox.Domain.Pickup.Models;

namespace ParcelBox.Application.Tests.TestContexts;

internal sealed class PickupCodeReissueTestContext
{
    public PickupCodeReissueService Service { get; set; }
    public PickupService PickupService { get; set; }
    public Parcel Parcel { get; set; }
    public Compartment Compartment { get; set; }
    public PickupAccess Access { get; set; }
    public UnitOfWorkFake UnitOfWork { get; set; }
    public MessageGatewayFake MessageGateway { get; set; }
    public PickupCodeServiceFake PickupCodes { get; set; }

    public PickupCodeReissueTestContext(PickupCodeReissueService service, PickupService pickupService, Parcel parcel, Compartment compartment, PickupAccess access, UnitOfWorkFake unitOfWork, MessageGatewayFake messageGateway, PickupCodeServiceFake pickupCodes)
    {
        Service = service;
        PickupService = pickupService;
        Parcel = parcel;
        Compartment = compartment;
        Access = access;
        UnitOfWork = unitOfWork;
        MessageGateway = messageGateway;
        PickupCodes = pickupCodes;
    }
}
