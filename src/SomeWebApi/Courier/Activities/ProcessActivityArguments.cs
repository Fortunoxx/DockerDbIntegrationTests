namespace SomeWebApi.Courier.Activities;

public record ProcessActivityArguments
{
    public string ProcessName { get; init; } = string.Empty;
    public int Id { get; init; }
}
