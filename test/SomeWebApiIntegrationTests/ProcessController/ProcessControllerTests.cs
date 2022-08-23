namespace SomeWebApiIntegrationTests.ProcessController;

using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SomeWebApi.Database;
using System.Net.Http;
using MassTransit;
using SomeWebApi.Courier.Activities;
using System;
using MassTransit.Testing;
using AutoFixture;
using Microsoft.Extensions.Logging.Abstractions;
using MassTransit.Courier.Contracts;

public sealed class ProcessControllerTests : IClassFixture<IntegrationTestFactory<Program, SqlServerContext>>
{
    private readonly IntegrationTestFactory<Program, SqlServerContext> _factory;
    // private readonly ActivityTestHarness<ProcessActivity, ProcessActivityArguments, ProcessActivityLog> testActivity;
    // private readonly ActivityTestHarness<TestFaultActivity, TestFaultActivity.TestFaultActivityArguments, TestFaultActivity.TestFaultActivityLog> faultingActivity;

    public ProcessControllerTests(IntegrationTestFactory<Program, SqlServerContext> factory)
    {
        _factory = factory;
    }

    // [Fact]
    // public async Task Start_Process_Should_Fail_Invalid_Id_Async()
    // {
    //     // Arrange 
    //     var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(0);
    //     var body = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
    //     var client = _factory.CreateClient();

    //     // Act
    //     var cut = async () => await client.PostAsync("process", body);

    //     // Assert
    //     await cut.Should().ThrowAsync<RoutingSlipException>(); //.WithMessage("The Id number is invalid*");
    // }

    [Fact]
    public async Task Start_Process_Should_Execute_RoutingSlip_Async()
    {
        // Arrange 
        var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(1);
        var body = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("process", body);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "process should have been started");
        response.EnsureSuccessStatusCode();
    }

    // https://stackoverflow.com/questions/62290941/testing-activity-compensate-with-masstransit
    [Fact]
    public async Task Compensation_Should_Work_Async()
    {
        // await using var provider = new ServiceCollection()
        //     .AddMassTransitTestHarness(x =>
        //     {
        //         x.SetKebabCaseEndpointNameFormatter();
        //         // x.AddActivity<ProcessActivity, ProcessActivityArguments, ProcessActivityLog>();
        //         // x.AddActivity<TestFaultActivity, TestFaultActivity.TestFaultActivityArguments, TestFaultActivity.TestFaultActivityLog>();
        //     })
        //     .BuildServiceProvider(true);

        // var harness = provider.GetTestHarness();

        // await harness.Start();


        var harness = new InMemoryTestHarness
        {
            TestTimeout = TimeSpan.FromSeconds(5)
        };
        var logger = NullLogger<ProcessActivity>.Instance;
        var testActivity = harness.Activity<ProcessActivity, ProcessActivityArguments, ProcessActivityLog>(
            _ => new ProcessActivity(logger),
            _ => new ProcessActivity(logger)
        );

        var faultingActivity = harness.Activity<TestFaultActivity, TestFaultActivity.TestFaultActivityArguments, TestFaultActivity.TestFaultActivityLog>(
            _ => new TestFaultActivity(),
            _ => new TestFaultActivity()
        );
        await harness.Start();

        try
        {
            var builder = new RoutingSlipBuilder(Guid.NewGuid());
            builder.AddSubscription(harness.Bus.Address, RoutingSlipEvents.All);

            var completed = harness.SubscribeHandler<RoutingSlipActivityCompleted>();
            var faulted = harness.SubscribeHandler<RoutingSlipActivityFaulted>();
            var compensated = harness.SubscribeHandler<RoutingSlipActivityCompensated>();
            var routingSlipCompleted = harness.SubscribeHandler<RoutingSlipCompleted>();
            var routingSlipFaulted = harness.SubscribeHandler<RoutingSlipFaulted>();

            var fixture = new Fixture();

            builder.AddActivity("Activity to be tested",
                testActivity.ExecuteAddress,
                fixture.Create<ProcessActivityArguments>());

            // builder.AddActivity("Faulting Activity",
            //     faultingActivity.ExecuteAddress,
            //     fixture.Create<TestFaultActivity.TestFaultActivityArguments>());

            var routingSlip = builder.Build();
            await harness.Bus.Execute(routingSlip);

            var completedContext = await completed;
            // var faultContext = await faulted;
            var routingSlipCompletedContext = await routingSlipCompleted;
            // await routingSlipFaulted;
            // await compensated;
        }
        finally
        {
            await harness.Stop();
        }
    }
}
