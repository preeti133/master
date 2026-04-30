namespace RallyAPI.SharedKernel.Abstractions;

/// <summary>
/// Provides access to current authenticated user information.
/// Lives in SharedKernel so any module can reference it without cross-module coupling.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? Phone { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }

    bool IsInRole(string role);
    bool IsCustomer { get; }
    bool IsRestaurant { get; }
    bool IsRider { get; }
    bool IsAdmin { get; }
}
