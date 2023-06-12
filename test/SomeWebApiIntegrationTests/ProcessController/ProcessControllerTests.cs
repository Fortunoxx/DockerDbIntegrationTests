namespace SomeWebApiIntegrationTests.ProcessController;

using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SomeWebApi.Database;
using System.Net.Http;
using System.Net.Mime;

public sealed class ProcessControllerTests : IClassFixture<IntegrationTestFactory<Program, SqlServerContext, AnotherSqlServerContext>>
{
    private readonly IntegrationTestFactory<Program, SqlServerContext, AnotherSqlServerContext> _factory;

    public ProcessControllerTests(IntegrationTestFactory<Program, SqlServerContext, AnotherSqlServerContext> factory)
    {
        _factory = factory;
    }

    [Fact]
    internal async Task Start_Process_Should_Execute_RoutingSlip_Async()
    {
        // Arrange 
        var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(1);
        var body = new StringContent(jsonBody, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json);
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("process", body);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "process should have been started");
        response.EnsureSuccessStatusCode();
    }
}
