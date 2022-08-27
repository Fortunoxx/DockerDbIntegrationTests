namespace SomeWebApi.Courier.Helper;

using MassTransit;

public class CourierService : ICourierService
{
    private readonly IEndpointNameFormatter endpointNameFormatter;

    public CourierService(IEndpointNameFormatter endpointNameFormatter)
    {
        this.endpointNameFormatter = endpointNameFormatter;
    }

    public Uri GetActivityAddress<TActivity, TArguments>()
        where TActivity : class, IExecuteActivity<TArguments>
        where TArguments : class
    {
        var name = endpointNameFormatter.ExecuteActivity<TActivity, TArguments>();
        return new Uri($"queue:{name}");
    }
}
