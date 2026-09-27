using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.Pickup;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.Repositories;
using ParcelBox.Application.Interfaces.Security;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Pickup;

namespace ParcelBox.Application.Services;

public sealed class PickupAccessService : IPickupAccessService
{
    private const int MaxFailedAttempts = 3;
    private static readonly TimeSpan AccessLifetime = TimeSpan.FromHours(24);

    private readonly IPickupAccessRepository _pickupAccesses;
    private readonly IPickupCodeService _pickupCodes;

    public PickupAccessService(
        IPickupAccessRepository pickupAccesses,
        IPickupCodeService pickupCodes)
    {
        _pickupAccesses = pickupAccesses;
        _pickupCodes = pickupCodes;
    }

    public async Task<Result<PickupPreparation, PickupOperationError>> CreateAsync(
        Guid parcelId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (parcelId == Guid.Empty)
        {
            return Result<PickupPreparation, PickupOperationError>.Failure(
                PickupOperationError.ParcelNotFound);
        }

        var code = _pickupCodes.Generate();
        var access = new PickupAccess
        {
            Id = Guid.NewGuid(),
            ParcelId = parcelId,
            CodeHash = _pickupCodes.Hash(code),
            ExpiresAt = now.Add(AccessLifetime),
            FailedAttempts = 0,
            Status = PickupStatus.Active
        };

        await _pickupAccesses.AddAsync(access, cancellationToken);

        return Result<PickupPreparation, PickupOperationError>.Success(
            new PickupPreparation(code));
    }

    public async Task<Result<PickupAccess, PickupOperationError>> ValidateAsync(
        Guid parcelId,
        string pickupCode,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var access = await _pickupAccesses.GetByParcelIdAsync(parcelId, cancellationToken);

        if (access is null)
        {
            return Result<PickupAccess, PickupOperationError>.Failure(
                PickupOperationError.PickupAccessNotFound);
        }

        if (access.Status == PickupStatus.Used)
        {
            return Result<PickupAccess, PickupOperationError>.Failure(
                PickupOperationError.PickupCodeUsed);
        }

        if (access.Status == PickupStatus.Locked)
        {
            return Result<PickupAccess, PickupOperationError>.Failure(
                PickupOperationError.PickupCodeLocked);
        }

        if (now >= access.ExpiresAt)
        {
            return Result<PickupAccess, PickupOperationError>.Failure(
                PickupOperationError.PickupCodeExpired);
        }

        if (_pickupCodes.Verify(pickupCode.Trim(), access.CodeHash))
        {
            return Result<PickupAccess, PickupOperationError>.Success(access);
        }

        access.FailedAttempts++;

        if (access.FailedAttempts >= MaxFailedAttempts)
        {
            access.Status = PickupStatus.Locked;
        }

        return Result<PickupAccess, PickupOperationError>.Failure(
            access.Status == PickupStatus.Locked
                ? PickupOperationError.PickupCodeLocked
                : PickupOperationError.PickupCodeInvalid);
    }

    public void MarkUsed(PickupAccess access, DateTimeOffset now)
    {
        access.Status = PickupStatus.Used;
        access.UsedAt = now;
    }
}
