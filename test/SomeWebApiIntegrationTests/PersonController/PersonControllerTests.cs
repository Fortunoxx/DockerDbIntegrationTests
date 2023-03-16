namespace SomeWebApiIntegrationTests.PersonController;

using System.Threading.Tasks;
using Xunit;
using AutoFixture;
using SomeWebApi.Model;
using System.Net.Http;
using FluentAssertions;
using SomeWebApi.Database;
using Newtonsoft.Json;
using System.Net.Mime;

public sealed class PersonControllerTests : IClassFixture<IntegrationTestFactory<Program, SqlServerContext>>
{
    private readonly IntegrationTestFactory<Program, SqlServerContext> _factory;

    public PersonControllerTests(IntegrationTestFactory<Program, SqlServerContext> factory)
    {
        _factory = factory;
    }

    [Fact]
    internal async Task GetPersons_Returns_Valid_Result_Async()
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
    internal async Task GetPerson_Returns_Valid_Result_Async()
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
    internal async Task Create_Person_Returns_Valid_Result_Async()
    {
        // Arrange 
        var user = new Fixture().Create<UpsertUser>();
        var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(user);
        var body = new StringContent(jsonBody, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json);
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("person", body);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created, "we expect a person to be created");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task Delete_Person_Returns_Valid_Result_Async()
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
    internal async Task Update_Person_Returns_Valid_Result_Async()
    {
        // Arrange 
        var client = _factory.CreateClient();
        var user = new Fixture().Create<UpsertUser>();
        var body = JsonConvert.SerializeObject(user);

        // Act
        var response = await client.PutAsync("person/2", new StringContent(body, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent, "this PUT method should have no content");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    internal async Task Delete_Person_Should_Fail_No_Person_Found_Async()
    {
        // Arrange 
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("person/-1");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound, "this person should not exist");
    }

    [Fact]
    internal async Task Update_Person_Should_Fail_No_Person_Found_Async()
    {
        // Arrange 
        var client = _factory.CreateClient();
        var user = new Fixture().Create<UpsertUser>();
        var body = JsonConvert.SerializeObject(user);

        // Act
        var response = await client.PutAsync("person/-2", new StringContent(body, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound, "this person should not exist");
    }
}
