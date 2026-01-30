using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RallyAPI.Integrations.ProRouting;
using RallyAPI.SharedKernel.Abstractions.Delivery;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;
using Xunit;

namespace RallyAPI.Integrations.ProRouting.Tests;

public class ProRoutingClientTests
{
    private readonly Mock<ILogger<ProRoutingClient>> _loggerMock;
    private readonly ProRoutingOptions _options;

    public ProRoutingClientTests()
    {
        _loggerMock = new Mock<ILogger<ProRoutingClient>>();
        _options = new ProRoutingOptions
        {
            BaseUrl = "https://test-api.com",
            ApiKey = "test-key",
            TimeoutSeconds = 30,
            DefaultOrderCategory = "F&B",
            DefaultSearchCategory = "Immediate Delivery",
            DefaultOrderWeight = 2,
            Enabled = true
        };
    }

    private DeliveryQuoteRequest CreateValidRequest()
    {
        return DeliveryQuoteRequest.Create(
            pickupLatitude: 12.921180,
            pickupLongitude: 77.588025,
            pickupPincode: "560041",
            dropLatitude: 12.920803,
            dropLongitude: 77.586608,
            dropPincode: "560041",
            city: "Bangalore",
            orderAmount: 200m,
            orderWeight: 2m);
    }

    [Fact]
    public async Task GetQuoteAsync_SuccessfulResponse_ReturnsSuccessResult()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var responseJson = JsonSerializer.Serialize(new
        {
            status = 1,
            estimate_id = "est_123456789",
            estimated_delivery_time = 30,
            price = 40
        });

        mockHttp
            .When(HttpMethod.Post, "https://test-api.com/partner/estimate")
            .Respond("application/json", responseJson);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri(_options.BaseUrl);

        var client = new ProRoutingClient(
            httpClient,
            Options.Create(_options),
            _loggerMock.Object);

        // Act
        var result = await client.GetQuoteAsync(CreateValidRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.QuoteId.Should().Be("est_123456789");
        result.Price.Should().Be(40);
        result.EstimatedMinutes.Should().Be(30);
        result.ProviderName.Should().Be("ProRouting");
    }

    [Fact]
    public async Task GetQuoteAsync_ApiReturnsError_ReturnsFailureResult()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var responseJson = JsonSerializer.Serialize(new
        {
            status = 0,
            message = "Invalid pincode"
        });

        mockHttp
            .When(HttpMethod.Post, "https://test-api.com/partner/estimate")
            .Respond("application/json", responseJson);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri(_options.BaseUrl);

        var client = new ProRoutingClient(
            httpClient,
            Options.Create(_options),
            _loggerMock.Object);

        // Act
        var result = await client.GetQuoteAsync(CreateValidRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid pincode");
    }

    [Fact]
    public async Task GetQuoteAsync_HttpError_ReturnsFailureResult()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        mockHttp
            .When(HttpMethod.Post, "https://test-api.com/partner/estimate")
            .Respond(HttpStatusCode.InternalServerError);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri(_options.BaseUrl);

        var client = new ProRoutingClient(
            httpClient,
            Options.Create(_options),
            _loggerMock.Object);

        // Act
        var result = await client.GetQuoteAsync(CreateValidRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("500");
    }

    [Fact]
    public async Task GetQuoteAsync_ProviderDisabled_ReturnsFailureResult()
    {
        // Arrange
        _options.Enabled = false;

        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri(_options.BaseUrl);

        var client = new ProRoutingClient(
            httpClient,
            Options.Create(_options),
            _loggerMock.Object);

        // Act
        var result = await client.GetQuoteAsync(CreateValidRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("disabled");
    }

    [Fact]
    public void ProviderName_ReturnsProRouting()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri(_options.BaseUrl);

        var client = new ProRoutingClient(
            httpClient,
            Options.Create(_options),
            _loggerMock.Object);

        // Assert
        client.ProviderName.Should().Be("ProRouting");
    }
}