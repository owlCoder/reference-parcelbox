namespace ParcelBox.Web.Contracts;

public sealed class ProblemDetailsResponseDto
{
    public string? Title { get; set; } = string.Empty;
    public string? Detail { get; set; } = string.Empty;
    public int? Status { get; set; }

    public ProblemDetailsResponseDto()
    {
    }

    public ProblemDetailsResponseDto(string? title, string? detail, int? status)
    {
        Title = title;
        Detail = detail;
        Status = status;
    }
}
