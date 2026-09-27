namespace ParcelBox.Application.Abstractions.Security;

public interface IPickupCodeService
{
    string Generate();

    string Hash(string code);

    bool Verify(string code, string hash);
}
