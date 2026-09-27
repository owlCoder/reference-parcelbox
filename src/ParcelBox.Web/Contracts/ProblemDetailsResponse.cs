namespace ParcelBox.Web.Contracts;

public sealed record ProblemDetailsResponse(
    string? Title,
    string? Detail,
    int? Status);
