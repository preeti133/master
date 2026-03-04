// File: src/Modules/Users/RallyAPI.Users.Application/Abstractions/ISmsService.cs
// Purpose: Abstraction for SMS delivery. Swap implementations without touching OTP logic.

namespace RallyAPI.Users.Application.Abstractions;

public interface ISmsService
{
    /// <summary>
    /// Sends an SMS message to the specified phone number.
    /// </summary>
    /// <param name="phoneNumber">E.164 format phone number (e.g., +919876543210)</param>
    /// <param name="message">The SMS body text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the SMS was accepted by the provider, false otherwise</returns>
    Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
