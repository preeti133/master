namespace RallyAPI.SharedKernel.Results;

/// <summary>
/// A field-level validation error, used in validation failure responses.
/// </summary>
public sealed record FieldError(string Field, string Message);
