// File: src/Modules/Users/RallyAPI.Users.Infrastructure/Services/ConsoleSmsService.cs
// Purpose: Development-only SMS service that logs OTPs to console instead of sending real SMS.
//          Registered automatically when environment is Development.

using Microsoft.Extensions.Logging;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Infrastructure.Services;

internal sealed class ConsoleSmsService : ISmsService
{
    private readonly ILogger<ConsoleSmsService> _logger;

    public ConsoleSmsService(ILogger<ConsoleSmsService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "╔══════════════════════════════════════════╗");
        _logger.LogWarning(
            "║  [DEV SMS] To: {Phone}", phoneNumber);
        _logger.LogWarning(
            "║  Message: {Message}", message);
        _logger.LogWarning(
            "╚══════════════════════════════════════════╝");

        return Task.FromResult(true);
    }
}
