using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RallyAPI.SharedKernel.Abstractions.Delivery;
using Xunit;

namespace RallyAPI.Integrations.ProRouting.Tests;

public class MockDeliveryQuoteProviderTests
{
    private readonly Mock<ILogger<MockDeliveryQuoteProvider>> _loggerMock;

    public MockDeliveryQuoteProviderTests()
    {
        _loggerMock = new Mock<ILogger<MockDeliveryQuoteProvider>>();
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
            orderAmount: 200m);
    }

    [Fact]
    public async Task GetQuoteAsync_DefaultOptions_ReturnsSuccess()
    {
        // Arrange
        var provider = new MockDeliveryQuoteProvider(_loggerMock.Object);

        // Act
        var result = await provider.GetQuoteAsync(CreateValidRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.QuoteId.Should().StartWith("mock_");
        result.Price.Should().BeGreaterThan(0);
        result.EstimatedMinutes.Should().Be(30);
        result.ProviderName.Should().Be("Mock");
    }

    [Fact]
    public async Task GetQuoteAsync_ConfiguredToFail_ReturnsFailure()
    {
        // Arrange
        var options = new MockQuoteOptions
        {
            ShouldFail = true,
            FailureMessage = "Test failure"
        };

        var provider = new MockDeliveryQuoteProvider(_loggerMock.Object, options);

        // Act
        var result = await provider.GetQuoteAsync(CreateValidRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Test failure");
    }

    [Fact]
    public async Task GetQuoteAsync_CustomPrice_ReturnsConfiguredPrice()
    {
        // Arrange
        var options = new MockQuoteOptions { BasePrice = 100m };
        var provider = new MockDeliveryQuoteProvider(_loggerMock.Object, options);

        // Act
        var result = await provider.GetQuoteAsync(CreateValidRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Price.Should().BeGreaterOrEqualTo(80m); // Allow some variation
    }

    [Fact]
    public void ProviderName_ReturnsMock()
    {
        // Arrange
        var provider = new MockDeliveryQuoteProvider(_loggerMock.Object);

        // Assert
        provider.ProviderName.Should().Be("Mock");
    }
}