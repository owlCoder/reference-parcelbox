using ParcelBox.Application.Enums;
using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Tests.Services;

public sealed class LockerServiceTests
{
    [Fact]
    public async Task AssignAsync_ChoosesSmallestCompatibleAvailableCompartment()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.AddRange(
        [
            NewCompartment("A1", SizeCategory.Small),
            NewCompartment("B1", SizeCategory.Medium),
            NewCompartment("C1", SizeCategory.Large)
        ]);

        var service = new LockerService(repository, new LockerControllerFake());
        var parcelId = Guid.NewGuid();

        var result = await service.AssignAsync(
            parcelId,
            SizeCategory.Medium,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("B1", result.Value.Number);
        Assert.Equal(CompartmentStatus.Occupied, result.Value.Status);
        Assert.Equal(parcelId, result.Value.ParcelId);
    }

    [Fact]
    public async Task AssignAsync_ReturnsNoCompatibleCompartment_WhenRequiredSizeIsUnavailable()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.Add(NewCompartment("A1", SizeCategory.Small));

        var service = new LockerService(repository, new LockerControllerFake());

        var result = await service.AssignAsync(
            Guid.NewGuid(),
            SizeCategory.Large,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LockerOperationError.NoCompatibleCompartment, result.Error);
    }


    [Fact]
    public async Task AssignAsync_ReservesDifferentCompartments_ForConcurrentParcels()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.Add(NewCompartment("B1", SizeCategory.Medium));
        repository.Items.Add(NewCompartment("B2", SizeCategory.Medium));
        var service = new LockerService(repository, new LockerControllerFake());

        var results = await Task.WhenAll(
            service.AssignAsync(Guid.NewGuid(), SizeCategory.Medium, CancellationToken.None),
            service.AssignAsync(Guid.NewGuid(), SizeCategory.Medium, CancellationToken.None));

        Assert.All(results, result => Assert.True(result.IsSuccess));
        Assert.NotEqual(results[0].Value.Id, results[1].Value.Id);
        Assert.All(repository.Items, compartment => Assert.Equal(CompartmentStatus.Occupied, compartment.Status));
    }

    [Fact]
    public async Task AssignAsync_DoesNotReuseClaimedCompartment_WhenControllerFails()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.Add(NewCompartment("B1", SizeCategory.Medium));
        var controller = new LockerControllerFake { Error = LockerControllerError.Jammed };
        var service = new LockerService(repository, controller);

        var first = await service.AssignAsync(Guid.NewGuid(), SizeCategory.Medium, CancellationToken.None);
        controller.Error = LockerControllerError.None;
        var second = await service.AssignAsync(Guid.NewGuid(), SizeCategory.Medium, CancellationToken.None);

        Assert.Equal(LockerOperationError.Jammed, first.Error);
        Assert.Equal(LockerOperationError.NoCompatibleCompartment, second.Error);
        Assert.Equal(CompartmentStatus.Reserved, repository.Items[0].Status);
    }

    private static Compartment NewCompartment(string number, SizeCategory size)
    {
        return new Compartment
        {
            Id = Guid.NewGuid(),
            LockerCode = "PB-01",
            Number = number,
            Size = size,
            Status = CompartmentStatus.Available
        };
    }
}
