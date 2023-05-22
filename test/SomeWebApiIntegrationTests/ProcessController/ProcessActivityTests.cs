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
    internal async Task Process_Should_Work_Async()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddActivity<ProcessActivity, ProcessActivityArguments, ProcessActivityLog>();
                x.AddActivity<TestFaultActivity, TestFaultActivity.TestFaultActivityArguments, TestFaultActivity.TestFaultActivityLog>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetTestHarness();

        await harness.Start();

        var builder = new RoutingSlipBuilder(Guid.NewGuid());
        builder.AddSubscription(harness.Bus.Address, RoutingSlipEvents.All);

        var fixture = new Fixture();

        builder.AddActivity("Activity to be tested",
            new Uri("loopback://localhost/process_execute"),
            fixture.Create<ProcessActivityArguments>());

        var routingSlip = builder.Build();
        await harness.Bus.Execute(routingSlip);

        (await harness.Sent.Any<RoutingSlipActivityCompleted>()).Should().BeTrue("first activity should succeed");
        (await harness.Sent.Any<RoutingSlipCompleted>()).Should().BeTrue("routing slip should be finished");
    }

    [Fact]
    internal async Task Process_Should_Fail_Invalid_Id_Async()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();
                    x.AddActivity<ProcessActivity, ProcessActivityArguments, ProcessActivityLog>();
                })
            .BuildServiceProvider(true);

        var harness = provider.GetTestHarness();

        await harness.Start();

        var builder = new RoutingSlipBuilder(Guid.NewGuid());
        builder.AddSubscription(harness.Bus.Address, RoutingSlipEvents.All);

        var fixture = new Fixture();

        builder.AddActivity("Activity to be tested",
            new Uri("loopback://localhost/process_execute"),
            fixture.Build<ProcessActivityArguments>().With(x => x.Id, 0).Create());

        var routingSlip = builder.Build();
        await harness.Bus.Execute(routingSlip);

        (await harness.Sent.Any<RoutingSlipActivityCompleted>()).Should().BeFalse("first activity should fail");
        (await harness.Sent.Any<RoutingSlipFaulted>()).Should().BeTrue("activity failed, so the routing slip failed");
    }

    [Fact]
    internal async Task Process_With_Compensation_Should_Work_Async()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddActivity<ProcessActivity, ProcessActivityArguments, ProcessActivityLog>();
                x.AddActivity<TestFaultActivity, TestFaultActivity.TestFaultActivityArguments, TestFaultActivity.TestFaultActivityLog>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetTestHarness();

        await harness.Start();

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
}
