using ParcelBox.Application.Interfaces.Persistence;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class UnitOfWorkFake : IUnitOfWork
{
    public int SaveCalls { get; private set; }
    public bool CanSave { get; set; } = true;

    public Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCalls++;
        return Task.FromResult(CanSave);
    }
}
