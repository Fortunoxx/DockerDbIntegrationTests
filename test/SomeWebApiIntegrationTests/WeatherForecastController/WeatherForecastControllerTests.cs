namespace SomeWebApiIntegrationTests.WeatherForecastController;

using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

public sealed class WeatherForecastControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WeatherForecastControllerTests(WebApplicationFactory<Program> factory)
        => _factory = factory;

    [Fact]
    internal async Task GetWeatherForecast_Returns_Valid_Result()
    {
        // Arrange 
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("WeatherForecast");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "we like success");
        response.EnsureSuccessStatusCode();
    }
}