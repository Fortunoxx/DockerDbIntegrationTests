namespace SomeWebApiIntegrationTests.ProcessController;

using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using MassTransit;
using SomeWebApi.Courier.Activities;
using System;
using MassTransit.Testing;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.DependencyInjection;
using AutoFixture;

public sealed class ProcessActivityTests
{
    [Fact]
    public async Task Process_Should_Work_Async()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitRabbitMqTestHarness()
            .AddMassTransitTestHarness(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddActivity<ProcessActivity, ProcessActivityArguments, ProcessActivityLog>();
                x.AddActivity<TestFaultActivity, TestFaultActivity.TestFaultActivityArguments, TestFaultActivity.TestFaultActivityLog>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetTestHarness();

        await harness.Start();

        try
        {
            var builder = new RoutingSlipBuilder(Guid.NewGuid());
            builder.AddSubscription(harness.Bus.Address, RoutingSlipEvents.All);

            var fixture = new Fixture();

            builder.AddActivity("Activity to be tested",
                new Uri("loopback://localhost/process_execute"),
                fixture.Create<ProcessActivityArguments>());

            var routingSlip = builder.Build();
            await harness.Bus.Execute(routingSlip);

            (await harness.Sent.Any<RoutingSlipActivityCompleted>()).Should().BeTrue();
            (await harness.Sent.Any<RoutingSlipCompleted>()).Should().BeTrue();
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Process_With_Compensation_Should_Work_Async()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitRabbitMqTestHarness()
            .AddMassTransitTestHarness(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddActivity<ProcessActivity, ProcessActivityArguments, ProcessActivityLog>();
                x.AddActivity<TestFaultActivity, TestFaultActivity.TestFaultActivityArguments, TestFaultActivity.TestFaultActivityLog>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetTestHarness();

        await harness.Start();

        try
        {
            var builder = new RoutingSlipBuilder(Guid.NewGuid());
            builder.AddSubscription(harness.Bus.Address, RoutingSlipEvents.All);

            var fixture = new Fixture();

            builder.AddActivity("Activity to be tested",
                new Uri("loopback://localhost/process_execute"),
                fixture.Create<ProcessActivityArguments>());

            builder.AddActivity("Faulting Activity",
                new Uri("loopback://localhost/test-fault_execute"),
                fixture.Create<TestFaultActivity.TestFaultActivityArguments>());

            var routingSlip = builder.Build();
            await harness.Bus.Execute(routingSlip);

            (await harness.Sent.Any<RoutingSlipActivityCompleted>()).Should().BeTrue("first activity should succeed");
            (await harness.Sent.Any<RoutingSlipActivityFaulted>()).Should().BeTrue("second activity should fail");
            (await harness.Sent.Any<RoutingSlipActivityCompensated>()).Should().BeTrue("first activity should be compensated");
            (await harness.Sent.Any<RoutingSlipFaulted>()).Should().BeTrue("second activity failed, so the routing slip failed too");
        }
        finally
        {
            await harness.Stop();
        }
    }
}
