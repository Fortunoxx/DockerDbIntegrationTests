namespace SomeWebApiIntegrationTests.PersonController;

using System.Threading.Tasks;
using Xunit;
using AutoFixture;
using SomeWebApi.Model;
using System.Net.Http;
using FluentAssertions;
using SomeWebApi.Database;
using Newtonsoft.Json;

public sealed class PersonControllerTests : IClassFixture<IntegrationTestFactory<Program, SqlServerContext>>
{
    private readonly IntegrationTestFactory<Program, SqlServerContext> _factory;

    public PersonControllerTests(IntegrationTestFactory<Program, SqlServerContext> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPersons_ResturnsValidResult_Async()
    {
        // Arrange 
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("person");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "there should be some persons");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetPerson_ResturnsValidResult_Async()
    {
        // Arrange 
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("person/2");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "a person with this id should exist");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Create_Person_ResturnsValidResult_Async()
    {
        // Arrange 
        var user = new Fixture().Create<UpsertUser>();
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
    public async Task Delete_Person_ResturnsValidResult_Async()
    {
        // Arrange 
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("person/1");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent, "a deleted ressource has no content");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Update_Person_ResturnsValidResult_Async()
    {
        // Arrange 
        var client = _factory.CreateClient();
        var user = new Fixture().Create<UpsertUser>();
        var body = JsonConvert.SerializeObject(user);

        // Act
        var response = await client.PutAsync("person/2", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent, "this PUT method should have no content");
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