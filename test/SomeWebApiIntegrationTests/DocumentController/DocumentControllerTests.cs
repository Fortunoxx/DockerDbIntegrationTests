namespace SomeWebApiIntegrationTests.DocumentController;

using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

public sealed class DocumentControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DocumentControllerTests(WebApplicationFactory<Program> factory)
        => _factory = factory;

    [Fact]
    internal async Task GeDocument_Returns_Valid_Result()
    {
        // Arrange 
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("document");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "we like success");
        response.EnsureSuccessStatusCode();
    }
}