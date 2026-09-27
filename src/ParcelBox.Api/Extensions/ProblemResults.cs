namespace ParcelBox.Api.Extensions;

internal static class ProblemResults
{
    public static IResult Create<TError>(
        TError error,
        int statusCode,
        string detail)
        where TError : struct, Enum
    {
        return Results.Problem(
            statusCode: statusCode,
            title: error.ToString(),
            detail: detail);
    }
}
