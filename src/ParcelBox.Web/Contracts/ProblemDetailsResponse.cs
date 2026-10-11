namespace ParcelBox.Web.Contracts;

public sealed class ProblemDetailsResponse
{
    public string? Title { get; set; } = string.Empty;
    public string? Detail { get; set; } = string.Empty;
    public int? Status { get; set; }

    public ProblemDetailsResponse()
    {
    }

    public ProblemDetailsResponse(string? title, string? detail, int? status)
    {
        Title = title;
        Detail = detail;
        Status = status;
    }
}
