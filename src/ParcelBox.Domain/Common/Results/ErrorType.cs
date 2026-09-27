namespace ParcelBox.Domain.Common.Results;

public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unavailable = 4,
    Failure = 5
}
