namespace SomeWebApi.Courier.Activities;

public record ProcessActivityArguments
{
    public string ProcessName { get; init; }
    public int Id { get; init; }
}
