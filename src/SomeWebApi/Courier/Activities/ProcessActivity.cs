namespace SomeWebApi.Courier.Activities;

using System.Linq;
using MassTransit;

public class ProcessActivity :
    IActivity<ProcessActivityArguments, ProcessActivityLog>
{
    readonly ILogger<ProcessActivity> logger;

    public ProcessActivity(ILogger<ProcessActivity> logger)
    {
        this.logger = logger;
    }

    public async Task<CompensationResult> Compensate(CompensateContext<ProcessActivityLog> context)
    {
        context.Log.LogIds.ToList().ForEach(x => logger.LogWarning($"Compensation: deleting id {x}"));
        return context.Compensated();
    }

    public async Task<ExecutionResult> Execute(ExecuteContext<ProcessActivityArguments> context)
    {
        logger.LogInformation("Verifying Id: {0}", context.Arguments.Id);

        // verify id with remote service
        if (context.Arguments.Id == 0)
        {
            throw new RoutingSlipException($"The Id number is invalid: {context.Arguments.Id}");
        }

        await Task.Delay(10);

        return context.Completed<ProcessActivityLog>(new { LogIds = new[] { Guid.NewGuid() } });
    }
}
