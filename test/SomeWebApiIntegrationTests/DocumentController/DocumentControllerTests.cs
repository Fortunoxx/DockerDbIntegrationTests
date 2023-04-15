namespace SomeWebApiIntegrationTests.DocumentController;

using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SomeWebApi.Contracts.APIs;
using NSubstitute;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

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

    [Fact]
    internal async Task GeDocumentByTemplate_Returns_Valid_Result()
    {
        // Arrange 
        var documentApi = Substitute.For<IDocumentApi>();
        var data = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        documentApi.GetPdfFileAsync(string.Empty).ReturnsForAnyArgs(Task.FromResult(data));
        
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => documentApi);
            });
        })
        .CreateClient();

        // Act
        var response = await client.GetAsync("document/template01");

        // Assert
        data.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "we like success");
        data.EnsureSuccessStatusCode();
    }
}