namespace SomeWebApi.Consumers;

using MassTransit;
using MassTransit.Courier.Contracts;
using SomeWebApi.Contracts;
using SomeWebApi.Courier.Activities;
using SomeWebApi.Courier.Events;
using SomeWebApi.Courier.Helper;

public class StartProcessCommandConsumer : IConsumer<IStartProcessCommand>
{
    private readonly ICourierService courierService;
    private readonly ILogger<StartProcessCommandConsumer> logger;

    public StartProcessCommandConsumer(ICourierService courierService, ILogger<StartProcessCommandConsumer> logger)
    {
        this.courierService = courierService;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<IStartProcessCommand> context)
    {
        var builder = new RoutingSlipBuilder(NewId.NextGuid());
        builder.AddActivity("StartProcess", courierService.GetActivityAddress<ProcessActivity, ProcessActivityArguments>(), new
        {
            ProcessName = context.Message.ProcessName,
            Id = context.Message.Id
        });

        await builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.ActivityFaulted, RoutingSlipEventContents.None, "StartProcess",
            x => x.Send<StartProcessFailed>(new { context.Message.Id, context.Message.ProcessName, }));

        var routingSlip = builder.Build();

        await context.Execute(routingSlip).ConfigureAwait(false);
        await context.RespondAsync<IAcceptedResponse>(new { Timestamp = DateTime.Now });
    }
}