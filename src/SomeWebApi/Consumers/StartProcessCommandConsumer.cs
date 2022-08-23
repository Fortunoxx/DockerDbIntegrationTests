namespace SomeWebApi.Consumers;

using MassTransit;
using SomeWebApi.Contracts;
using SomeWebApi.Courier.Activities;
using SomeWebApi.Courier.Helper;

public class StartProcessCommandConsumer : IConsumer<IStartProcessCommand>
{
    private readonly ICourierService courierHelper;
    private readonly IEndpointNameFormatter endpointNameFormatter;
    private readonly ILogger<StartProcessCommandConsumer> logger;

    public StartProcessCommandConsumer(ICourierService courierHelper, IEndpointNameFormatter endpointNameFormatter, ILogger<StartProcessCommandConsumer> logger)
    {
        this.courierHelper = courierHelper;
        this.endpointNameFormatter = endpointNameFormatter;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<IStartProcessCommand> context)
    {
        var builder = new RoutingSlipBuilder(NewId.NextGuid());
        builder.AddActivity("StartProcess", courierHelper.GetActivityAddress<ProcessActivity, ProcessActivityArguments>(), new
        {
            ProcessName = context.Message.ProcessName,
            Id = context.Message.Id
        });
        var routingSlip = builder.Build();

        await context.Execute(routingSlip).ConfigureAwait(false);
        await context.RespondAsync<IAcceptedResponse>(new { Timestamp = DateTime.Now });
    }
}