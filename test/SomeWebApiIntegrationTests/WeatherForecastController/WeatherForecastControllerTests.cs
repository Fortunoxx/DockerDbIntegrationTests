namespace SomeWebApiIntegrationTests.WeatherForecastController;

using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SomeWebApi.Database;

public sealed class WeatherForecastControllerTests : IClassFixture<IntegrationTestFactory<Program, SqlServerContext>>
{
    private readonly IntegrationTestFactory<Program, SqlServerContext> _factory;

    public WeatherForecastControllerTests(IntegrationTestFactory<Program, SqlServerContext> factory)
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