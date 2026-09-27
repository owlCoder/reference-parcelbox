using ParcelBox.Application.Enums;
using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers;

namespace ParcelBox.Application.Tests;

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
