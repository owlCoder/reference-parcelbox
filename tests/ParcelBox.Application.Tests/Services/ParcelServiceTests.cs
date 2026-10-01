using ParcelBox.Application.DTOs.Parcels;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Services;
using ParcelBox.Application.Tests.TestDoubles;
using ParcelBox.Domain.Common.Enums;
using ParcelBox.Domain.Parcels.Enums;
using ParcelBox.Domain.Parcels.Models;

namespace ParcelBox.Application.Tests.Services;

public sealed class ParcelServiceTests
{
    private static readonly DateTimeOffset Now =
        DateTimeOffset.Parse("2026-01-01T10:00:00+00:00");

    [Fact]
    public async Task RegisterAsync_CreatesRegisteredParcel()
    {
        var parcels = new ParcelRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var service = new ParcelService(
            parcels,
            unitOfWork,
            new FixedTimeProvider(Now));

        var result = await service.RegisterAsync(
            new RegisterParcelInput("PKG-001", "+38160111222", SizeCategory.Medium),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ParcelStatus.Registered, result.Value.Status);
        Assert.Equal(Now, result.Value.CreatedAt);
        Assert.Single(parcels.Items);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateTrackingCode()
    {
        var parcels = new ParcelRepositoryFake();
        parcels.Items.Add(new Parcel
        {
            Id = Guid.NewGuid(),
            TrackingCode = "PKG-001",
            RecipientPhone = "+38160111222",
            Size = SizeCategory.Small,
            Status = ParcelStatus.Registered,
            CreatedAt = Now
        });

        var service = new ParcelService(
            parcels,
            new UnitOfWorkFake(),
            new FixedTimeProvider(Now));

        var result = await service.RegisterAsync(
            new RegisterParcelInput("PKG-001", "+38160111333", SizeCategory.Medium),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ParcelOperationError.TrackingCodeAlreadyExists, result.Error);
    }
}
