using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Lockers.Enums;
using ParcelBox.Domain.Lockers.Models;

namespace ParcelBox.Application.Tests;

public sealed class CompartmentServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsCompartmentDetails()
    {
        var repository = new CompartmentRepositoryFake();
        repository.Items.Add(new Compartment
        {
            Id = Guid.NewGuid(),
            LockerCode = "PB-01",
            Number = "A1",
            Size = SizeCategory.Small,
            Status = CompartmentStatus.Available
        });

        var service = new CompartmentService(repository);

        var result = await service.GetAllAsync(CancellationToken.None);

        var compartment = Assert.Single(result);
        Assert.Equal("PB-01", compartment.LockerCode);
        Assert.Equal("A1", compartment.Number);
        Assert.Equal(CompartmentStatus.Available, compartment.Status);
    }
}
