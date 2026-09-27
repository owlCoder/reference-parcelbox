using System.Security.Cryptography;
using System.Text;
using ParcelBox.Application.Abstractions;

namespace ParcelBox.Infrastructure.Security;

internal sealed class PickupCodeService : IPickupCodeService
{
    public string Generate() => RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

    public string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }
}
