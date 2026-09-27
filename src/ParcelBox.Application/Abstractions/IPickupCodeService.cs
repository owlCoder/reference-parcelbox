namespace ParcelBox.Application.Abstractions;

public interface IPickupCodeService
{
    string Generate();
    string Hash(string code);
}
