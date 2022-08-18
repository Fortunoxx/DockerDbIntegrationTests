namespace SomeWebApiIntegrationTests.PersonController;

using System.Threading.Tasks;
using Xunit;
using AutoFixture;
using SomeWebApi.Model;
using System.Net.Http;
using FluentAssertions;
using SomeWebApi.Database;

public sealed class PersonControllerTests : IClassFixture<IntegrationTestFactory<Program, SqlServerContext>>
{
    private readonly IntegrationTestFactory<Program, SqlServerContext> _factory;

    public PersonControllerTests(IntegrationTestFactory<Program, SqlServerContext> factory) 
    {
        _factory = factory;
    }
        // => _factory = factory;

    [Fact]
    public async Task Create_User_ResturnsValidResult()
    {
        // Arrange 
        var user = new Fixture().Create<UserForCreate>();
        var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(user);
        var body = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("person", body);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created, "we expect a person to be created");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Delete_User_ResturnsValidResult()
    {
        // Arrange 
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("person/1");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent, "a deleted ressource has no content");
        response.EnsureSuccessStatusCode();
    }
}

public sealed class WeatherForecastControllerTests : IClassFixture<IntegrationTestFactory<Program, SqlServerContext>>
{
    private readonly IntegrationTestFactory<Program, SqlServerContext> _factory;

    public WeatherForecastControllerTests(IntegrationTestFactory<Program, SqlServerContext> factory) 
        => _factory = factory;

    [Fact]
    public async Task GetWeatherForecast_ResturnsValidResult()
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