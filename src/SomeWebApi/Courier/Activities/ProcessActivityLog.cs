namespace SomeWebApi.Courier.Activities;

public record ProcessActivityLog
{
    public IEnumerable<Guid> LogIds { get; init; } = new List<Guid>();
}