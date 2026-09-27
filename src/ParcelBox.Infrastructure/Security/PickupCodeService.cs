using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ParcelBox.Application.Interfaces.Security;

namespace ParcelBox.Infrastructure.Security;

internal sealed class PickupCodeService : IPickupCodeService
{
    public string Generate()
    {
        return RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString(CultureInfo.InvariantCulture);
    }

    public string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string code, string hash)
    {
        var actual = Encoding.UTF8.GetBytes(Hash(code));
        var expected = Encoding.UTF8.GetBytes(hash);

        return actual.Length == expected.Length
            && CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
