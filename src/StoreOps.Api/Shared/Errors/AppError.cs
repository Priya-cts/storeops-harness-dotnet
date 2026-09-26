namespace StoreOps.Api.Shared.Errors;

/// <summary>
/// Typed error hierarchy for StoreOps. Architecture rule (Section 3.5, "Error contract"):
/// no raw `throw new Error()` / `throw new Exception()` is permitted in services or routes.
/// Every thrown error must derive from AppError so the ExceptionHandlingMiddleware can
/// translate it into a consistent { code, message } response with the correct status code.
/// </summary>
public abstract class AppError : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    protected AppError(string code, string message, int statusCode) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

public sealed class NotFoundError : AppError
{
    public NotFoundError(string resource, object id)
        : base("NOT_FOUND", $"{resource} with id '{id}' was not found.", StatusCodes.Status404NotFound)
    {
    }
}

public sealed class ValidationError : AppError
{
    public ValidationError(string message)
        : base("VALIDATION_ERROR", message, StatusCodes.Status400BadRequest)
    {
    }
}

public sealed class ForbiddenError : AppError
{
    public ForbiddenError(string message)
        : base("FORBIDDEN", message, StatusCodes.Status403Forbidden)
    {
    }
}

public sealed class ConflictError : AppError
{
    public ConflictError(string message)
        : base("CONFLICT", message, StatusCodes.Status409Conflict)
    {
    }
}
