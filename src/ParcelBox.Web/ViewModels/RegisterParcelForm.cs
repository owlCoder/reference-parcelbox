using ParcelBox.Web.Enums;

namespace ParcelBox.Web.ViewModels;

public sealed class RegisterParcelForm
{
    public string TrackingCode { get; set; } = "PKG-001";

    public string RecipientPhone { get; set; } = "+38160111222";

    public SizeCategory Size { get; set; } = SizeCategory.Medium;
}
