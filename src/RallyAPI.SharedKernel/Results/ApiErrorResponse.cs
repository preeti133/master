namespace RallyAPI.SharedKernel.Results;

/// <summary>
/// Canonical error response shape returned by every endpoint and the global exception handler.
///
/// Always serialises to:
/// {
///   "error":   "Order.NotFound",
///   "message": "Human-readable description",
///   "details": []           // null / omitted when empty
/// }
/// </summary>
public sealed record ApiErrorResponse(
    string Error,
    string Message,
    IReadOnlyList<FieldError>? Details = null);
