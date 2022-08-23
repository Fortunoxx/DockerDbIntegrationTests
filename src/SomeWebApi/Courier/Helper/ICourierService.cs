namespace SomeWebApi.Courier.Helper;

using MassTransit;

public interface ICourierService
{
    Uri GetActivityAddress<TActivity, TArguments>()
        where TActivity : class, IExecuteActivity<TArguments>
        where TArguments : class;
}