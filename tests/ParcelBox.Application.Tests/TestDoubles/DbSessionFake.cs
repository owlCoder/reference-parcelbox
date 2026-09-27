using ParcelBox.Application.Abstractions.Persistence;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class DbSessionFake : IAppDbSession
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(1);
    }
}
