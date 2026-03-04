// File: src/Modules/Users/RallyAPI.Users.Infrastructure/Services/ExotelSmsService.cs
//
// Exotel SMS API (from docs):
//   POST https://<api_key>:<api_token>@<subdomain>/v1/Accounts/<sid>/Sms/send
//   Content-Type: application/x-www-form-urlencoded
//
//   Required: From, To, Body
//   India-mandatory: DltEntityId
//   Recommended for OTP: Priority=high, SmsType=transactional, EncodingType=plain

using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Infrastructure.Services;

internal sealed class ExotelSmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly ExotelOptions _options;
    private readonly ILogger<ExotelSmsService> _logger;

    public ExotelSmsService(
        HttpClient httpClient,
        IOptions<ExotelOptions> options,
        ILogger<ExotelSmsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // HTTP Basic Auth: ApiKey as username, ApiToken as password
        // Exotel docs: https://<api_key>:<api_token>@<subdomain>/v1/...
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_options.ApiKey}:{_options.ApiToken}"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<bool> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var formattedPhone = FormatPhoneForExotel(phoneNumber);

            // URL: https://<subdomain>/v1/Accounts/<sid>/Sms/send
            var url = $"https://{_options.Subdomain}/v1/Accounts/{_options.AccountSid}/Sms/send";

            // Build form data with all required + recommended fields
            var formFields = new List<KeyValuePair<string, string>>
            {
                new("From", _options.SenderId),
                new("To", formattedPhone),
                new("Body", message),
                new("EncodingType", "plain"),
                new("Priority", "high"),          // OTP = high priority
                new("SmsType", "transactional"),  // OTP is transactional
            };

            // DLT Entity ID — mandatory for India
            if (!string.IsNullOrWhiteSpace(_options.DltEntityId))
            {
                formFields.Add(new("DltEntityId", _options.DltEntityId));
            }

            // DLT Template ID — optional but recommended
            if (!string.IsNullOrWhiteSpace(_options.DltTemplateId))
            {
                formFields.Add(new("DltTemplateId", _options.DltTemplateId));
            }

            var formData = new FormUrlEncodedContent(formFields);

            _logger.LogInformation(
                "Sending SMS via Exotel to {Phone} (formatted: {ExotelPhone})",
                MaskPhone(phoneNumber),
                MaskPhone(formattedPhone));

            var response = await _httpClient.PostAsync(url, formData, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "SMS sent successfully to {Phone}. Status: {StatusCode}",
                    MaskPhone(phoneNumber),
                    (int)response.StatusCode);
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Exotel SMS failed for {Phone}. Status: {StatusCode}, Response: {ErrorBody}",
                MaskPhone(phoneNumber),
                (int)response.StatusCode,
                errorBody);

            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error sending SMS via Exotel to {Phone}",
                MaskPhone(phoneNumber));
            return false;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Exotel SMS request timed out for {Phone}",
                MaskPhone(phoneNumber));
            return false;
        }
    }

    /// <summary>
    /// Formats phone for Exotel. Docs say "Preferably in E.164 format"
    /// but examples show 0-prefixed. We'll send E.164 (+91XXXXXXXXXX)
    /// and let Exotel handle it, as their docs say they'll try to match.
    /// </summary>
    private static string FormatPhoneForExotel(string phone)
    {
        // Strip everything except digits and +
        var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // If already E.164 format (+91...), return as-is
        if (cleaned.StartsWith("+91") && cleaned.Length == 13)
            return cleaned;

        // If 91XXXXXXXXXX without +
        if (cleaned.StartsWith("91") && cleaned.Length == 12)
            return $"+{cleaned}";

        // If raw 10 digits
        if (cleaned.Length == 10 && !cleaned.StartsWith('0'))
            return $"+91{cleaned}";

        // If 0-prefixed 11 digits (remove 0, add +91)
        if (cleaned.StartsWith('0') && cleaned.Length == 11)
            return $"+91{cleaned[1..]}";

        // Fallback — return as-is and let Exotel figure it out
        return cleaned;
    }

    /// <summary>
    /// Masks phone for logging — shows last 4 digits only.
    /// </summary>
    private static string MaskPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4)
            return "****";
        return $"****{phone[^4..]}";
    }
}
