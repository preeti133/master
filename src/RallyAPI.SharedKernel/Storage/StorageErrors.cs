using RallyAPI.SharedKernel.Results;

namespace RallyAPI.SharedKernel.Storage;

public static class StorageErrors
{
    public static Error InvalidFileType(string provided) =>
        Error.Validation($"File type '{provided}' is not allowed. Accepted: image/jpeg, image/png, image/webp.");

    public static Error FileTooLarge(long bytes) =>
        Error.Validation($"File size {bytes / 1024 / 1024:F1}MB exceeds the 5MB limit.");

    public static readonly Error UploadUrlGenerationFailed =
        Error.Validation("Failed to generate upload URL. Please try again.");

    public static readonly Error InvalidFileKey =
        Error.Validation("The provided file key is invalid or does not belong to this resource.");

    public static Error DeleteFailed(string key) =>
        Error.Validation($"Failed to delete file '{key}' from storage.");
}