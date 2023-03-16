namespace SomeWebApiIntegrationTests.ProcessController;

using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SomeWebApi.Consumers;
using SomeWebApi.Contracts;
using SomeWebApi.Courier.Helper;
using Xunit;

public sealed class ProcessConsumerTests
{
    [Fact]
    internal async Task StartProcessCommandConsumer_Should_Succed_Async()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddTransient<ICourierService, CourierService>();
                x.AddConsumer<StartProcessCommandConsumer>();
                x.AddLogging(_ => new NullLogger<StartProcessCommandConsumer>());
            })
            .BuildServiceProvider(true);

        var harness = provider.GetTestHarness();

        await harness.Start();

        // Act
        var cut = harness.GetRequestClient<IStartProcessCommand>();
        var res = cut.GetResponse<IAcceptedResponse>(new { Id = 1, ProcessName = "TestProcess" });

        // Assert
        (await harness.Sent.Any<IAcceptedResponse>()).Should().BeTrue();
        (await harness.Consumed.Any<IStartProcessCommand>()).Should().BeTrue();

        var consumerHarness = harness.GetConsumerHarness<StartProcessCommandConsumer>();

        (await consumerHarness.Consumed.Any<IStartProcessCommand>()).Should().BeTrue();
    }
}
