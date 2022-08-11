namespace SomeWebApiIntegrationTests.PersonController;

using System.Threading.Tasks;
using Xunit;
using AutoFixture;
using SomeWebApi.Model;
using System.Net.Http;
using FluentAssertions;
using SomeWebApi.Database;

public sealed class GetPersonControllerTests : IClassFixture<IntegrationTestFactory<Program, SqlServerContext>>
{
    private readonly IntegrationTestFactory<Program, SqlServerContext> _factory;

    public GetPersonControllerTests(IntegrationTestFactory<Program, SqlServerContext> factory) 
        => _factory = factory;

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
        // response.Should().BeEquivalentTo(user);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
    }
}