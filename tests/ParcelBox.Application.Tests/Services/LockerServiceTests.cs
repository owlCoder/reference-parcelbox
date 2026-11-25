using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Application.Services;
using ParcelBox.Application.Services.CompartmentSelection;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Tests.Services;

public sealed class LockerServiceTests
{
    [Fact]
    public async Task AssignAsync_Compact_ChoosesSmallestCompatibleAvailableCompartment()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.AddRange(
        [
            NewCompartment("PB-01", "A1", SizeCategory.Small),
            NewCompartment("PB-01", "B1", SizeCategory.Medium),
            NewCompartment("PB-01", "C1", SizeCategory.Large)
        ]);

        var service = CreateService(repository, new LockerControllerFake());

        var result = await service.AssignAsync(
            Guid.NewGuid(),
            SizeCategory.Medium,
            CompartmentSelectionStrategy.Compact,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("B1", result.Value.Compartment.Number);
        Assert.Equal(CompartmentStatus.Occupied, result.Value.Compartment.Status);
    }

    [Fact]
    public async Task AssignAsync_ReturnsNoCompatibleCompartment_WhenRequiredSizeIsUnavailable()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.Add(NewCompartment("PB-01", "A1", SizeCategory.Small));
        var service = CreateService(repository, new LockerControllerFake());

        var result = await service.AssignAsync(
            Guid.NewGuid(),
            SizeCategory.Large,
            CompartmentSelectionStrategy.Compact,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LockerOperationError.NoCompatibleCompartment, result.Error);
    }

    [Fact]
    public async Task AssignAsync_ReservesDifferentCompartments_ForConcurrentParcels()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.Add(NewCompartment("PB-01", "B1", SizeCategory.Medium));
        repository.Items.Add(NewCompartment("PB-01", "B2", SizeCategory.Medium));
        var service = CreateService(repository, new LockerControllerFake());

        var results = await Task.WhenAll(
            service.AssignAsync(Guid.NewGuid(), SizeCategory.Medium, CompartmentSelectionStrategy.Compact, CancellationToken.None),
            service.AssignAsync(Guid.NewGuid(), SizeCategory.Medium, CompartmentSelectionStrategy.Compact, CancellationToken.None));

        Assert.All(results, result => Assert.True(result.IsSuccess));
        Assert.NotEqual(results[0].Value.Compartment.Id, results[1].Value.Compartment.Id);
        Assert.All(repository.Items, compartment =>
            Assert.Equal(CompartmentStatus.Occupied, compartment.Status));
    }

    [Fact]
    public async Task AssignAsync_DoesNotReuseClaimedCompartment_WhenControllerFails()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.Add(NewCompartment("PB-01", "B1", SizeCategory.Medium));
        var controller = new LockerControllerFake { Error = LockerControllerError.Jammed };
        var service = CreateService(repository, controller);

        var first = await service.AssignAsync(
            Guid.NewGuid(),
            SizeCategory.Medium,
            CompartmentSelectionStrategy.Compact,
            CancellationToken.None);

        controller.Error = LockerControllerError.None;

        var second = await service.AssignAsync(
            Guid.NewGuid(),
            SizeCategory.Medium,
            CompartmentSelectionStrategy.Compact,
            CancellationToken.None);

        Assert.Equal(LockerOperationError.Jammed, first.Error);
        Assert.Equal(LockerOperationError.NoCompatibleCompartment, second.Error);
        Assert.Equal(CompartmentStatus.Reserved, repository.Items[0].Status);
    }

    [Fact]
    public async Task AssignAsync_PreserveLarge_PrefersLockerWithFewerAvailableLargeCompartments()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.Add(NewCompartment("PB-01", "B1", SizeCategory.Medium));
        repository.Items.Add(NewCompartment("PB-01", "C1", SizeCategory.Large));
        repository.Items.Add(NewCompartment("PB-01", "C2", SizeCategory.Large));
        repository.Items.Add(NewCompartment("PB-02", "B1", SizeCategory.Medium));
        repository.Items.Add(NewCompartment("PB-02", "C1", SizeCategory.Large));
        var service = CreateService(repository, new LockerControllerFake());

        var result = await service.AssignAsync(
            Guid.NewGuid(),
            SizeCategory.Medium,
            CompartmentSelectionStrategy.PreserveLarge,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("PB-02", result.Value.Compartment.LockerCode);
        Assert.Equal(SizeCategory.Medium, result.Value.Compartment.Size);
    }

    [Fact]
    public async Task AssignAsync_PreserveLarge_UsesLargeWhenNoOtherCompatibleCompartmentIsAvailable()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.Add(NewCompartment("PB-01", "C1", SizeCategory.Large));
        var service = CreateService(repository, new LockerControllerFake());

        var result = await service.AssignAsync(
            Guid.NewGuid(),
            SizeCategory.Small,
            CompartmentSelectionStrategy.PreserveLarge,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SizeCategory.Large, result.Value.Compartment.Size);
    }

    [Fact]
    public async Task AssignAsync_Balanced_PrefersLeastUtilizedLocker()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.Add(NewCompartment("PB-01", "B1", SizeCategory.Medium));
        repository.Items.Add(new Compartment
        {
            Id = Guid.NewGuid(),
            LockerCode = "PB-01",
            Number = "A1",
            Size = SizeCategory.Small,
            Status = CompartmentStatus.Occupied,
            ParcelId = Guid.NewGuid()
        });
        repository.Items.Add(NewCompartment("PB-02", "B1", SizeCategory.Medium));
        repository.Items.Add(NewCompartment("PB-02", "C1", SizeCategory.Large));
        var service = CreateService(repository, new LockerControllerFake());

        var result = await service.AssignAsync(
            Guid.NewGuid(),
            SizeCategory.Medium,
            CompartmentSelectionStrategy.Balanced,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("PB-02", result.Value.Compartment.LockerCode);
        Assert.Equal(SizeCategory.Medium, result.Value.Compartment.Size);
    }

    private static LockerService CreateService(
        CompartmentRepositoryFake repository,
        LockerControllerFake controller)
    {
        ICompartmentSelectionStrategy[] strategies =
        [
            new CompactCompartmentSelectionStrategy(),
            new PreserveLargeCompartmentSelectionStrategy(),
            new BalancedCompartmentSelectionStrategy()
        ];

        return new LockerService(repository, controller, strategies);
    }

    private static Compartment NewCompartment(
        string lockerCode,
        string number,
        SizeCategory size)
    {
        return new Compartment
        {
            Id = Guid.NewGuid(),
            LockerCode = lockerCode,
            Number = number,
            Size = size,
            Status = CompartmentStatus.Available
        };
    }
}
