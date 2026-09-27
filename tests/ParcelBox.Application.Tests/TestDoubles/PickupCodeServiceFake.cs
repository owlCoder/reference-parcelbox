using ParcelBox.Application.Abstractions.Security;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class PickupCodeServiceFake : IPickupCodeService
{
    public string Generate()
    {
        return "123456";
    }

    public string Hash(string code)
    {
        return $"hash:{code}";
    }

    public bool Verify(string code, string hash)
    {
        return Hash(code) == hash;
    }
}
