using ParcelBox.Application.Interfaces.Persistence;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class UnitOfWorkFake : IUnitOfWork
{
    public int SaveCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCalls++;
        return Task.FromResult(1);
    }
}
