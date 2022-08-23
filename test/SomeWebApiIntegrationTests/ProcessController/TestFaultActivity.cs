namespace SomeWebApiIntegrationTests.ProcessController;

using System.Threading.Tasks;
using MassTransit;
using System;

public class TestFaultActivity : IActivity<TestFaultActivity.TestFaultActivityArguments, TestFaultActivity.TestFaultActivityLog>
{
    public Task<CompensationResult> Compensate(CompensateContext<TestFaultActivityLog> context)
    {
        throw new NotImplementedException();
    }

    public Task<ExecutionResult> Execute(ExecuteContext<TestFaultActivityArguments> context)
    {
        throw new NotImplementedException();
    }

    public record TestFaultActivityArguments { }
    public record TestFaultActivityLog { }
}
