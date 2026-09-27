using ParcelBox.Application.Interfaces.Security;

namespace ParcelBox.Application.Tests.TestDoubles;

internal sealed class PickupCodeServiceFake : IPickupCodeService
{
    public string Code { get; set; } = "123456";

    public string Generate()
    {
        return Code;
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
